using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace adventofcode
{
    public class LeaderboardParser
    {
        private string _jsonFileName;
        private int _leaderBoardId;
        private int _year;
        private string _htmlFileName;
        private int[] _years;

        public LeaderboardParser()
        {
            _settings = File.ReadAllLines("..\\..\\settings.txt")
                .Where(s => !string.IsNullOrEmpty(s.Trim()) && !s.StartsWith("//") && s.Contains("="))
                .Select(l => l.Split(new[] { '=' }, 2))
                .ToDictionary(x => x[0].Trim(), x => x[1].Trim());
        }

        public void GenerateReport(int leaderboardid, int year)
        {
            GenerateReport(leaderboardid, year, new[] { year });
        }

        private void GenerateReport(int leaderboardid, int year, int[] years)
        {
            _years = years;

            var interval = TimeSpan.FromHours(new Random().Next(2, 5));
            if (year == DateTime.Now.Year)
            {
                interval = TimeSpan.FromMinutes(15);
                if (DateTime.Now.Hour == 6 || DateTime.Now.Hour == 7)
                    interval = TimeSpan.FromMinutes(2);
            }

            _year = year;
            _leaderBoardId = leaderboardid;
            _htmlFileName = $"leaderboard_{_leaderBoardId}_{_year}.html";
            _jsonFileName = $"..\\..\\leaderboard_{leaderboardid}_{_year}.json";
            var updatedData = DownloadIfOld(interval);

            var leaderboard = ParseJson();

            DeriveMoreStats(leaderboard);

            Log($"Generating new html for {_leaderBoardId}/{_year}");
            var modified = GenerateHtml(leaderboard);
            if (updatedData || modified)
                UploadStatistics();
        }

        private void UploadStatistics()
        {
            Log($"Uploading html for {_leaderBoardId}/{_year}");
            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(_settings["ftpuser"], _settings["ftppwd"]);
                var targetUri = _settings["ftptarget"] + _htmlFileName;
                client.UploadFile(
                    targetUri,
                    WebRequestMethods.Ftp.UploadFile,
                    _htmlFileName);
            }
        }

        private bool GenerateHtml(LeaderBoard leaderboard)
        {
            var tabs = new Dictionary<string, StringBuilder>();
            var scripts = new Dictionary<string, StringBuilder>();

            var logAccumulatedPosition = InitLog("AccumulatedPosition", leaderboard.HighestDay);

            foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
            {
                if (p.TotalScore > 0)
                {
                    logAccumulatedPosition.Append("<tr class=\"item\">" + Cell(p.Name));


                    for (int day = 0; day < leaderboard.HighestDay; day++)
                        for (int star = 0; star < 2; star++)
                        {
                            var pos = p.AccumulatedPosition[day][star];
                            var medal = GetMedalClass(p.PositionForStar[day][star]);
                            if (p.PositionForStar[day][star] == -1)
                                medal = "gray";
                            if (pos != -1)
                                logAccumulatedPosition.Append(Cell(pos + 1, medal));
                            else
                            {
                                logAccumulatedPosition.Append(EmptyCell());
                            }
                        }

                    logAccumulatedPosition.AppendLine("</tr>");
                }
            }

            ExitLog(logAccumulatedPosition);
            tabs["Local leaderboard (Pos)"] = logAccumulatedPosition;

            //==
            var logScoreDiff = InitLog("Points behind the leader", leaderboard.HighestDay);

            foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
            {
                if (p.TotalScore > 0)
                {
                    logScoreDiff.Append("<tr class=\"item\">" + Cell(p.Name));

                    for (int day = 0; day < leaderboard.HighestDay; day++)
                        for (int star = 0; star < 2; star++)
                        {
                            var pos = p.AccumulatedPosition[day][star];
                            var medal = GetMedalClass(p.PositionForStar[day][star]);
                            if (p.PositionForStar[day][star] == -1)
                                medal = "gray";
                            if (pos != -1)
                            {
                                var value = leaderboard.TopScorePerDay[day][star] - p.AccumulatedScore[day][star];
                                logScoreDiff.Append(Cell(value, medal));
                            }
                            else
                            {
                                logScoreDiff.Append(EmptyCell());
                            }
                        }

                    logScoreDiff.AppendLine("</tr>");
                }
            }
            ExitLog(logScoreDiff);
            tabs["ScoreDiff"] = logScoreDiff;

            var logScoreDiffGraphHtml = InitGraphHtml("PointsBehindTheLeader");
            var logScoreDiffGraphScript = InitLineChartScript(leaderboard, "PointsBehindTheLeader");
            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                for (int star = 0; star < 2; star++)
                {
                    logScoreDiffGraphScript.Append("    [" + ((day * 2 + star + 1) / 2.0).ToString(CultureInfo.InvariantCulture) + ",");
                    foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
                    {
                        if (p.TotalScore > 0)
                        {
                            var value = leaderboard.TopScorePerDay[day][star] - p.AccumulatedScore[day][star];
                            logScoreDiffGraphScript.Append(value + ",");
                        }
                    }
                    logScoreDiffGraphScript.AppendLine("],");
                }
            }

            ExitLineChartScript(logScoreDiffGraphScript, "PointsBehindTheLeader", "Points behind the leader", true);
            tabs["ScoreDiff chart"] = logScoreDiffGraphHtml;
            scripts["ScoreDiff chart"] = logScoreDiffGraphScript;
            //==


            var logPositionForStar = InitLog("Position for star", leaderboard.HighestDay);
            var logOffsetFromWinner = InitLog("Offset from winner", leaderboard.HighestDay);
            var logTotalSolveTime = InitLog("Total Solve Time", leaderboard.HighestDay);
            var logAccumulatedScore = InitLog("Accumulated score", leaderboard.HighestDay);

            foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
            {
                if (p.TotalScore > 0)
                {
                    logPositionForStar.Append("<tr class=\"item\">" + Cell(p.Name));
                    logOffsetFromWinner.Append("<tr class=\"item\">" + Cell(p.Name));
                    logTotalSolveTime.Append("<tr class=\"item\">" + Cell(p.Name));
                    logAccumulatedScore.Append("<tr class=\"item\">" + Cell(p.Name));

                    for (int day = 0; day < leaderboard.HighestDay; day++)
                        for (int star = 0; star < 2; star++)
                        {
                            var pos = p.PositionForStar[day][star];
                            var medal = GetMedalClass(pos);

                            if (pos != -1)
                            {
                                logPositionForStar.Append(Cell(pos + 1, medal));
                                logTotalSolveTime.Append(Cell(p.TimeToComplete[day][star].Value, medal));
                                if (pos == 0)
                                    logOffsetFromWinner.Append(Cell("Winner", 0, true, medal));
                                else
                                {
                                    var value = p.OffsetFromWinner[day][star].Value;
                                    logOffsetFromWinner.Append(Cell("+" + value, (int)value.TotalSeconds, true, medal));
                                }
                            }
                            else
                            {
                                logPositionForStar.Append(EmptyCell());
                                logOffsetFromWinner.Append(EmptyCell());
                                logTotalSolveTime.Append(EmptyCell());
                            }


                            if (p.AccumulatedScore[day][star] != -1)

                            {
                                if (p.PositionForStar[day][star] == -1)
                                    medal = "gray";
                                logAccumulatedScore.Append(Cell(p.AccumulatedScore[day][star], medal));
                            }
                            else
                            {
                                logAccumulatedScore.Append(EmptyCell());
                            }
                        }

                    logPositionForStar.AppendLine("</tr>");
                    logOffsetFromWinner.AppendLine("</tr>");
                    logTotalSolveTime.AppendLine("</tr>");
                    logAccumulatedScore.AppendLine("</tr>");
                }
            }

            ExitLog(logPositionForStar);
            ExitLog(logOffsetFromWinner);
            ExitLog(logTotalSolveTime);
            ExitLog(logAccumulatedScore);

            tabs["Daily position"] = logPositionForStar;
            tabs["Offset"] = logOffsetFromWinner;
            tabs["Time"] = logTotalSolveTime;
            tabs["Local leaderboard (score)"] = logAccumulatedScore;


            // https://developers.google.com/chart/interactive/docs/gallery/linechart
            var logPosGraphHtml = InitGraphHtml("AccumulatedPosition");
            var logPosGraphScript = InitLineChartScript(leaderboard, "AccumulatedPosition");

            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                for (int star = 0; star < 2; star++)
                {
                    logPosGraphScript.Append("    [" + ((day * 2 + star + 1) / 2.0).ToString(CultureInfo.InvariantCulture) + ",");
                    foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
                    {
                        if (p.TotalScore > 0)
                        {
                            var pos = p.AccumulatedPosition[day][star] + 1;
                            logPosGraphScript.Append((pos != 0 ? pos.ToString() : "") + ",");
                        }
                    }

                    logPosGraphScript.AppendLine("],");
                }
            }

            ExitLineChartScript(logPosGraphScript, "AccumulatedPosition", "Position");
            tabs["Position chart"] = logPosGraphHtml;
            scripts["Position chart"] = logPosGraphScript;

            // https://developers.google.com/chart/interactive/docs/gallery/linechart
            var logDailyPosGraphHtml = InitGraphHtml("DailyPosition");
            var logDailyPosGraphScript = InitLineChartScript(leaderboard, "DailyPosition");

            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                for (int star = 0; star < 2; star++)
                {
                    logDailyPosGraphScript.Append(
                        "    [" + ((day * 2 + star + 1) / 2.0).ToString(CultureInfo.InvariantCulture) + ",");
                    foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
                    {
                        if (p.TotalScore > 0)
                        {
                            var pos = p.PositionForStar[day][star] + 1;
                            logDailyPosGraphScript.Append((pos != 0 ? pos.ToString() : "") + ",");
                        }
                    }

                    logDailyPosGraphScript.AppendLine("],");
                }
            }

            ExitLineChartScript(logDailyPosGraphScript, "DailyPosition", "Daily Position");
            tabs["Daily Position chart"] = logDailyPosGraphHtml;
            scripts["Daily Position chart"] = logDailyPosGraphScript;


            var logTimeStar2 = InitLog("Time to complete second star", leaderboard.HighestDay, false);

            foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
            {
                if (p.TotalScore > 0)
                {
                    logTimeStar2.Append("<tr class=\"item\">" + Cell(p.Name));
                    for (int day = 0; day < leaderboard.HighestDay; day++)
                    {
                        var pos = leaderboard.Players.Where(x => x.TimeToCompleteStar2[day].HasValue).OrderBy(x => x.TimeToCompleteStar2[day].Value).ToList().IndexOf(p);
                        var medal = GetMedalClass(pos);

                        if (p.TimeToCompleteStar2[day].HasValue)
                            logTimeStar2.Append(Cell(p.TimeToCompleteStar2[day].Value, medal));
                        else
                        {
                            logTimeStar2.Append(EmptyCell());
                        }
                    }

                    logTimeStar2.AppendLine("</tr>");
                }
            }

            ExitLog(logTimeStar2);

            tabs["Star2"] = logTimeStar2;


            //==

            var htmlContent = new StringBuilder();
            var html = File.ReadAllText("..\\..\\Leaderboard.template");
            html = html.Replace("$(GENDATE)", DateTime.Now.ToString());
            html = html.Replace("$(DATADATE)", File.GetLastWriteTime(_jsonFileName).ToString());

            htmlContent.AppendLine("<div class=\"w3-container\">");
            htmlContent.AppendLine("<div id=\"tabmenu\" class=\"w3-bar w3-black\">");
            var first = true;
            foreach (var k in tabs.Keys)
            {
                var red = first ? " w3-red" : "";
                htmlContent.AppendLine(
                    $"    <button class=\"w3-bar-item w3-button tablink{red}\" onclick=\"openTab(event,'{k}')\">{k}</button>");
                first = false;
            }

            htmlContent.AppendLine("<div align='right'>");
            foreach (var year in _years.OrderBy(y => y))
                if (year != _year)
                    htmlContent.Append($"<a href='leaderboard_{_leaderBoardId}_{year}.html'>{year}</a> ");
            htmlContent.AppendLine("</div>");

            htmlContent.AppendLine("</div>");

            first = true;
            foreach (var k in tabs.Keys)
            {
                var display = first ? "" : " style=\"display:none\"";
                htmlContent.AppendLine($"<div id=\"{k}\" class=\"w3-container w3-border tab\"{display}>");
                htmlContent.AppendLine(tabs[k].ToString());
                htmlContent.AppendLine($"</div>");
                first = false;
            }

            htmlContent.AppendLine("</div>");

            html = html.Replace("$(DATA)", htmlContent.ToString());

            var scriptContent = new StringBuilder();
            foreach (var k in scripts.Keys)
                scriptContent.AppendLine(scripts[k].ToString());

            html = html.Replace("$(SCRIPT)", scriptContent.ToString());
            var oldHtml = File.ReadAllText(_htmlFileName, Encoding.UTF8);

            var length = oldHtml.Length / 2;
            var oldhtml2 = oldHtml.Substring(0, length);
            var html2 = html.Substring(0, oldhtml2.Length);

            if (!File.Exists(_htmlFileName) || oldhtml2 != html2)
            {
                File.WriteAllText(_htmlFileName, html, Encoding.UTF8);
                return true;
            }

            return false;

        }

        private static string GetMedalClass(int pos)
        {
            var medal = "";
            switch (pos)
            {
                case 0:
                    medal = "gold";
                    break;
                case 1:
                    medal = "silver";
                    break;
                case 2:
                    medal = "bronze";
                    break;
            }

            return medal;
        }

        private bool DownloadIfOld(TimeSpan interval)
        {

            if (File.GetLastWriteTime(_jsonFileName) + interval < DateTime.Now)
            {
                Log($"Downloading new data for {_leaderBoardId}/{_year}");
                // Create Target
                var cookies = new CookieContainer();
                var destination = $"https://adventofcode.com/{_year}/leaderboard/private/view/{_leaderBoardId}.json";

                // Create a WebRequest object and assign it a cookie container and make them think your Mozilla ;)
                cookies.Add(new Cookie("session", _settings["cookie_"+_leaderBoardId], "/", ".adventofcode.com"));
                var webRequest = (HttpWebRequest)WebRequest.Create(destination);
                webRequest.Method = "GET";
                webRequest.Accept = "*/*";
                webRequest.AllowAutoRedirect = false;
                webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
                webRequest.CookieContainer = cookies;
                webRequest.ContentType = "text/xml";
                webRequest.Credentials = null;

                // Grab the response from the server for the current WebRequest
                var webResponse = (HttpWebResponse)webRequest.GetResponse();

                TextReader tr = new StreamReader(webResponse.GetResponseStream());

                var s = tr.ReadToEnd();
                if (File.Exists(_jsonFileName) && File.ReadAllText(_jsonFileName) == s)
                    return false;
                if (!string.IsNullOrEmpty(s))
                    File.WriteAllText(_jsonFileName, s);
                return true;
            }

            return false;
        }

        private StringBuilder InitGraphHtml(string graphName)
        {
            var chartHtml = new StringBuilder();
            chartHtml.AppendLine($"  <div class=\"chart\" id=\"chart_{graphName}\"></div>");
            return chartHtml;
        }

        private void ExitLineChartScript(StringBuilder chartScript, string chartName, string title, bool vAxisLog = false)
        {
            chartScript.AppendLine("  ]);");
            chartScript.AppendLine("  var chartwidth = $('#tabmenu').width();");
            chartScript.AppendLine("  var options = { width: chartwidth, height: 1000, left: 20,top: 20, hAxis: { title: 'day/star' }, explorer: {zoomDelta: 1.1}, vAxis: { logScale: '" + vAxisLog + "', title: '" + title + "' }};");
            chartScript.AppendLine($"  var chart = new google.visualization.LineChart(document.getElementById('chart_{chartName}'));");
            chartScript.AppendLine("  chart.draw(data, options);");
            chartScript.AppendLine("};");
            chartScript.AppendLine($" $(window).resize(function(){{draw{chartName}Chart();}});");
        }

        private static StringBuilder InitLineChartScript(LeaderBoard leaderboard, string chartName)
        {
            var chartScript = new StringBuilder();
            chartScript.AppendLine($"google.charts.setOnLoadCallback(draw{chartName}Chart);");
            chartScript.AppendLine($"function draw{chartName}Chart() {{");
            chartScript.AppendLine("  var data = new google.visualization.DataTable();");
            chartScript.AppendLine("  data.addColumn('number', 'X');");
            foreach (var p in leaderboard.Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar))
                if (p.TotalScore > 0)
                    chartScript.AppendLine($"  data.addColumn('number', '{p.Name}');");

            chartScript.AppendLine("  data.addRows([");
            return chartScript;
        }

        private void DeriveMoreStats(LeaderBoard leaderboard)
        {
            var bestTime = new TimeSpan[leaderboard.HighestDay][];
            var lastStar = new Dictionary<Player, long>();
            foreach (var player in leaderboard.Players)
            {
                lastStar[player] = -1;
            }

            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                var publishTime = new DateTime(_year, 12, day + 1, 6, 0, 0);
                bestTime[day] = new TimeSpan[2];
                foreach (var player in leaderboard.Players)
                {
                    for (int star = 0; star < 2; star++)
                    {
                        var unixStarTime = player.unixCompletionTime[day][star];
                        if (unixStarTime != -1)
                        {
                            var starTime = DateTimeOffset.FromUnixTimeSeconds(unixStarTime).DateTime.ToLocalTime();
                            var timeSpan = starTime - publishTime;
                            player.TimeToComplete[day][star] = timeSpan;
                            if (bestTime[day][star] == TimeSpan.Zero || timeSpan < bestTime[day][star])
                                bestTime[day][star] = timeSpan;
                        }
                    }

                    player.TimeToCompleteStar2[day] = player.TimeToComplete[day][1] - player.TimeToComplete[day][0];
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderboard.Players.Where(p => p.unixCompletionTime[day][star] != -1)
                        .OrderBy(p => p.unixCompletionTime[day][star]).ThenBy(p => lastStar[p]).ToList();
                    foreach (var player in leaderboard.Players)
                    {
                        if (player.unixCompletionTime[day][star] != -1)
                        {
                            player.PositionForStar[day][star] = orderedPlayers.IndexOf(player);
                            if (day != 5) // points day 6 were recalled 
                                player.TotalScore += leaderboard.Players.Count - player.PositionForStar[day][star];
                            player.OffsetFromWinner[day][star] = player.TimeToComplete[day][star] - bestTime[day][star];
                            lastStar[player] = player.unixCompletionTime[day][star];
                        }

                        player.AccumulatedScore[day][star] = player.TotalScore;
                        if (player.TotalScore > leaderboard.TopScorePerDay[day][star])
                            leaderboard.TopScorePerDay[day][star] = player.TotalScore;


                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderboard.Players.Where(p => p.AccumulatedScore[day][star] != 0)
                        .OrderByDescending(p => p.AccumulatedScore[day][star]).ToList();
                    foreach (var player in leaderboard.Players)
                    {
                        player.AccumulatedPosition[day][star] = orderedPlayers.IndexOf(player);
                    }
                }
            }
        }

        private LeaderBoard ParseJson()
        {
            var highestDay = -1;
            var players = new List<Player>();
            var json = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(_jsonFileName));
            var jmembers = json.Property("members").First();
            foreach (JProperty jmember in jmembers)
            {
                var jmemberdata = ((JObject)jmember.Value);
                var player = new Player
                {
                    Id = jmember.Name,
                    GlobalScore = GetInt(jmemberdata, "global_score"),
                    LocalScore = GetInt(jmemberdata, "local_score"),
                    Stars = GetInt(jmemberdata, "stars"),
                    LastStar = GetInt(jmemberdata, "last_star_ts"),
                    Name = GetStr(jmemberdata, "name")
                };
                if (player.Name == null)
                    player.Name = "anonymous " + player.Id;
                var x = jmemberdata.Property("completion_day_level");
                foreach (JProperty daydata in x.First)
                {
                    var day = int.Parse(daydata.Name);
                    highestDay = Math.Max(highestDay, day);
                    foreach (JProperty stardata in daydata.Value)
                    {
                        var starnum = int.Parse(stardata.Name);
                        var starts = GetInt((JObject)stardata.Value, "get_star_ts");
                        player.unixCompletionTime[day - 1][starnum - 1] = starts;
                    }
                }

                players.Add(player);
            }

            return new LeaderBoard(players, highestDay);
        }

        private static string Cell(string value, int sort, bool alignRight = false, string htmlClass = "")
        {
            var align = alignRight ? " align='right'" : "";
            htmlClass = htmlClass == "" ? "" : " class='" + htmlClass + "'";
            return $"<td sort='{sort}'{align}{htmlClass}>{value}</td>";
        }
        private static string Cell(string value)
        {
            return $"<td>{value}</td>";
        }
        private static string Cell(int value, string htmlClass = "")
        {
            return Cell(value.ToString(), value, true, htmlClass);
        }
        private static string Cell(TimeSpan value, string htmlClass)
        {
            return Cell(value.ToString(), (int)value.TotalSeconds, true, htmlClass);
        }
        private static string EmptyCell()
        {
            return Cell("", int.MaxValue);
        }

        private int tableid = 0;
        private Dictionary<string, string> _settings;

        private StringBuilder InitLog(string name, int highestDay, bool colForStar = true)
        {
            var starCols = colForStar ? 2 : 1;
            //            var randomid = _r.Next();
            tableid++;
            var log = new StringBuilder($"<h1>{name}</h1><div style=\"overflow-x:auto;\"> <table id=\"table_{tableid}\"><tr>");
            log.AppendLine($"<th>{name}</th>");
            for (int day = 0; day < highestDay; day++)
                for (int star = 0; star < starCols; star++)
                {
                    var colPos = day * starCols + star + 1;
                    var stars = new string('*', star + 1);
                    var cellContent = $"Day {day + 1}<br>{stars}";
                    log.AppendLine($"<th onclick=\"sortTable({colPos}, 'table_{tableid}')\" align='right' class='sortable'>{cellContent}</th>");
                }
            log.AppendLine("</tr>");
            return log;
        }

        private void ExitLog(StringBuilder log)
        {
            log.AppendLine("</table></div>");
        }

        protected void Log(string s)
        {
            Console.WriteLine(s);
            Debug.WriteLine(s);
        }


        private static long GetInt(JObject jmemberdata, string propname)
        {
            var jToken = jmemberdata.Property(propname).Value;
            var jValue = (JValue)jToken;
            var value = jValue.Value;
            if (value is string)
                return long.Parse(value.ToString());

            return (long)(jValue.Value);
        }
        private static string GetStr(JObject jmemberdata, string propname)
        {
            return (string)((JValue)jmemberdata.Property(propname).Value).Value;
        }

        public void GenerateReport(int leaderboardid, int[] years)
        {
            foreach (var year in years)
                GenerateReport(leaderboardid, year, years);
        }

    }
}