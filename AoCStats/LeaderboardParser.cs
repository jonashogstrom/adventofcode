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

namespace AoCStats
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
            var temp = File.ReadAllLines("..\\..\\settings.txt")
                .Where(s => !string.IsNullOrEmpty(s.Trim()) && !s.StartsWith("//") && s.Contains("="))
                .Select(l => l.Split(new[] { '=' }, 2)).ToList();
            var groups = temp.GroupBy(y => y[0]).Where(g => g.Count() > 1);
            foreach (var g in groups)
                Console.WriteLine("Dupe: " + g.Key);

            _settings = temp.ToDictionary(x => x[0].Trim(), x => x[1].Trim());
        }

        public void GenerateReport(int leaderboardid, int year, bool handleExcludes)
        {
            GenerateReport(leaderboardid, year, new[] { year }, handleExcludes, false);
        }

        private void GenerateReport(int leaderboardid, int year, int[] years, bool handleExcludes, bool forceLoad)
        {
            _years = years;
            _handleExcludes = handleExcludes;
            _x_suffix = (handleExcludes ? "_X" : "") + (_excludeZero ? "" : "_0");

            var interval = TimeSpan.FromHours(24);
            if (year == DateTime.Now.Year)
            {
                interval = TimeSpan.FromMinutes(15);
                if (DateTime.Now.Hour == 6 || DateTime.Now.Hour == 7)
                    interval = TimeSpan.FromMinutes(2);
            }

            _year = year;
            _leaderBoardId = leaderboardid;
            var fieldName = "fields_" + _leaderBoardId;
            if (_settings.ContainsKey(fieldName))
                _metacolumns = _settings["fields_" + _leaderBoardId].Split(',').Select(s => s.Trim()).ToList();
            else
                _metacolumns = new List<string>();

            _htmlFileName = $"leaderboard_{_leaderBoardId}_{_year}{_x_suffix}.html";
            _jsonFileName = $"..\\..\\leaderboard_{leaderboardid}_{_year}.json";
            var updatedData = DownloadIfOld(interval, forceLoad);

            var leaderboard = ParseJson();
            if (leaderboard != null)
            {
                for (int d = 1; d <= leaderboard.HighestDay; d++)
                    DownloadGlobalLeaderboard(year, d);

                DeriveMoreStats(leaderboard);
                ParseGlobalBoards(leaderboard, year);

                Log($"Generating new html for {_settings[_leaderBoardId + "_name"]}/{_year} ({_leaderBoardId})");
                var modified = GenerateHtml(leaderboard);
                if (updatedData || modified)
                    UploadStatistics();
            }
        }

        private void ParseGlobalBoards(LeaderBoard leaderboard, int year)
        {
            for (int i = 1; i <= leaderboard.HighestDay; i++)
            {
                var html = File.ReadAllText($"global_{year}_{i}.html").Split(new string[] { "<div class=\"leaderboard-entry\">" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var player in leaderboard.Players)
                {
                    var star = 1;
                    foreach (var r in html)
                    {
                        if (r.Contains($"height=\"20\"/></span>{player.Name}"))
                        {
                            var marker = "<span class=\"leaderboard-position\">";
                            var p = r.IndexOf(marker);
                            var pos = int.Parse(r.Substring(p + marker.Length, 3));
                            //                            Console.WriteLine($"{player.Name} scored {101 - pos} points on day {i} (star {star + 1}), year {year}");
                            player.GlobalScoreForDay[i - 1][star] = 101 - pos;
                        }
                        if (r.Contains("<span class=\"leaderboard-daydesc-first\">"))
                            star = 0;
                    }
                }
            }
        }

        private void DownloadGlobalLeaderboard(int year, int day)
        {
            var fileName = $"global_{year}_{day}.html";
            if (!File.Exists(fileName))
            {
                var s = DownloadFromURL($"https://adventofcode.com/{year}/leaderboard/day/{day}");
                File.WriteAllText(fileName, s);
            }
        }

        private void UploadStatistics()
        {
            Log($"Uploading html for {_settings[_leaderBoardId + "_name"]}/{_year} ({_leaderBoardId})");
            try
            {
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool GenerateHtml(LeaderBoard leaderboard)
        {
            var tabs = new Dictionary<string, StringBuilder>();
            var scripts = new Dictionary<string, StringBuilder>();

            var logAccumulatedPosition = InitLog("Leaderboard/Local score (pos)", leaderboard.HighestDay);

            foreach (var p in leaderboard.OrderedPlayers)
            {
                if (!_excludeZero || p.TotalScore > 0)
                {
                    logAccumulatedPosition.Append(AddStartOfRowAndNameCell(p));


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

                    logAccumulatedPosition.AppendLine(EndOfRow(p));
                }
            }

            ExitLog(logAccumulatedPosition);
            tabs["00-Leaderboard (Pos)"] = logAccumulatedPosition;

            //==
            var logScoreDiff = InitLog("Points behind the leader (local score)", leaderboard.HighestDay);

            foreach (var p in leaderboard.OrderedPlayers)
            {
                if (!_excludeZero || p.TotalScore > 0)
                {
                    logScoreDiff.Append(AddStartOfRowAndNameCell(p));

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

                    logScoreDiff.AppendLine(EndOfRow(p));
                }
            }
            ExitLog(logScoreDiff);
            tabs["10-ScoreDiff"] = logScoreDiff;

            var logScoreDiffGraphHtml = InitGraphHtml("PointsBehindTheLeader");
            var logScoreDiffGraphScript = InitLineChartScript(leaderboard, "PointsBehindTheLeader");
            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                for (int star = 0; star < 2; star++)
                {
                    logScoreDiffGraphScript.Append("    [" + ((day * 2 + star + 1) / 2.0).ToString(CultureInfo.InvariantCulture) + ",");
                    foreach (var p in leaderboard.OrderedPlayers)
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
            tabs["11-ScoreDiff chart"] = logScoreDiffGraphHtml;
            scripts["ScoreDiff chart"] = logScoreDiffGraphScript;
            //==


            var logPositionForStar = InitLog("Daily position", leaderboard.HighestDay);
            var logOffsetFromWinner = InitLog("Offset from winner", leaderboard.HighestDay);
            var logTotalSolveTime = InitLog("Total time to solve since the problem was published", leaderboard.HighestDay);
            var logAccumulatedSolveTime = InitLog("Accumulated time to solve", leaderboard.HighestDay);
            var logAccumulatedScore = InitLog("Leaderboard/Local score (accumulated score)", leaderboard.HighestDay);
            var logGlobalScore = InitLog("Leaderboard/Global score (score)", leaderboard.HighestDay);

            foreach (var p in leaderboard.OrderedPlayers)
            {
                if (!_excludeZero || p.TotalScore > 0)
                {
                    logPositionForStar.Append(AddStartOfRowAndNameCell(p));
                    logOffsetFromWinner.Append(AddStartOfRowAndNameCell(p));
                    logTotalSolveTime.Append(AddStartOfRowAndNameCell(p));
                    logAccumulatedSolveTime.Append(AddStartOfRowAndNameCell(p));
                    logAccumulatedScore.Append(AddStartOfRowAndNameCell(p));
                    logGlobalScore.Append(AddStartOfRowAndNameCell(p));

                    for (int day = 0; day < leaderboard.HighestDay; day++)
                        for (int star = 0; star < 2; star++)
                        {
                            var pos = p.PositionForStar[day][star];
                            var medal = GetMedalClass(pos);

                            if (p.AccumulatedTimeToComplete[day][star].HasValue)
                                logAccumulatedSolveTime.Append(Cell(p.AccumulatedTimeToComplete[day][star].Value, medal));
                            else
                                logAccumulatedSolveTime.Append(EmptyCell());

                            if (pos != -1)
                            {
                                logPositionForStar.Append(Cell(pos + 1, medal));
                                var time = p.TimeToComplete[day][star].Value;
                                logTotalSolveTime.Append(Cell(time, medal, time.TotalSeconds.ToString()));
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

                            if (p.GlobalScoreForDay[day][star].HasValue)
                                logGlobalScore.Append(Cell(
                                    p.GlobalScoreForDay[day][star].Value.ToString(),
                                    -p.GlobalScoreForDay[day][star].Value,
                                    true,
                                    "",
                                    "Position: " + (100 - p.GlobalScoreForDay[day][star].Value + 1)));
                            else
                            {
                                logGlobalScore.Append(EmptyCell());
                            }


                            if (p.AccumulatedScore[day][star] != -1)

                            {
                                if (p.PositionForStar[day][star] == -1)
                                    medal = "gray";
                                logAccumulatedScore.Append(Cell(p.AccumulatedScore[day][star], medal));
                            }
                            else
                            {
                                logAccumulatedScore.Append(EmptyCell(int.MinValue));
                            }
                        }

                    logPositionForStar.AppendLine(EndOfRow(p));
                    logOffsetFromWinner.AppendLine(EndOfRow(p));
                    logTotalSolveTime.AppendLine(EndOfRow(p));
                    logAccumulatedSolveTime.AppendLine(EndOfRow(p));
                    logAccumulatedScore.AppendLine(EndOfRow(p));
                    logGlobalScore.AppendLine(EndOfRow(p));
                }
            }

            ExitLog(logPositionForStar);
            ExitLog(logOffsetFromWinner);
            ExitLog(logTotalSolveTime);
            ExitLog(logAccumulatedSolveTime);
            ExitLog(logAccumulatedScore);
            ExitLog(logGlobalScore);

            tabs["02-Leaderboard (score)"] = logAccumulatedScore;
            tabs["06-Time"] = logTotalSolveTime;
            tabs["07-Offset"] = logOffsetFromWinner;
            tabs["08-Accumulated"] = logAccumulatedSolveTime;
            tabs["20-Global score"] = logGlobalScore;
            tabs["25-Daily position"] = logPositionForStar;

            // https://developers.google.com/chart/interactive/docs/gallery/linechart
            var logPosGraphHtml = InitGraphHtml("AccumulatedPosition");
            var logPosGraphScript = InitLineChartScript(leaderboard, "AccumulatedPosition");

            for (int day = 0; day < leaderboard.HighestDay; day++)
            {
                for (int star = 0; star < 2; star++)
                {
                    logPosGraphScript.Append("    [" + ((day * 2 + star + 1) / 2.0).ToString(CultureInfo.InvariantCulture) + ",");
                    foreach (var p in leaderboard.OrderedPlayers)
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
            tabs["01-Position chart"] = logPosGraphHtml;
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
                    foreach (var p in leaderboard.OrderedPlayers)
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
            tabs["44-Daily Position chart"] = logDailyPosGraphHtml;
            scripts["Daily Position chart"] = logDailyPosGraphScript;


            var logTimeStar2 = InitLog("Time to complete second star", leaderboard.HighestDay, false);

            foreach (var p in leaderboard.OrderedPlayers)
            {
                if (!_excludeZero || p.TotalScore > 0)
                {
                    logTimeStar2.Append(AddStartOfRowAndNameCell(p));
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

                    logTimeStar2.AppendLine(EndOfRow(p));
                }
            }

            ExitLog(logTimeStar2);

            tabs["09-Time *2"] = logTimeStar2;

            var tobiiScore = InitLog("Leaderboard/TobiiScore (accumulated score)", leaderboard.HighestDay, true);
            

            var tobiiPos = 0;
            foreach (var p in leaderboard.Players.OrderByDescending(p => p.Stars).ThenBy(p => p.AccumulatedTobiiScoreTotal).ThenBy(p=>p.LastStar))
            {
                tobiiPos++;
                if (!_excludeZero || p.TotalScore > 0)
                {
                    tobiiScore.Append(AddStartOfRowAndNameCell(p, tobiiPos));

                    for (int day = 0; day < leaderboard.HighestDay; day++)
                        for (int star = 0; star < 2; star++)
                        {
                            var medal = GetMedalClass(p.PositionForStar[day][star]);
                            if (p.PositionForStar[day][star] != -1)
                            {
                                tobiiScore.Append(Cell(p.AccumulatedTobiiScore[day][star], medal));
                            }
                            else
                            {
                                tobiiScore.Append(EmptyCell());
                            }
                        }

                    tobiiScore.AppendLine(EndOfRow(p));
                }
            }

            ExitLog(tobiiScore);
            tobiiScore.Append(
                "<hr>TobiiScore leaderboard is ordered by <br><ol><li>Completed stars</li><li>TobiiScore</li><li>Time for last star</li></ol>" +
                "TobiiScore calculated as the number of players on the list with a better result for each star. That means that a gold medal gives 0 points, silver gives 1 point etc. <br>" +
                "The advantage of this is that the score does not change if new players enters the list, and the ordering method encourages participants to solve all puzzles");

            tabs["99-TobiiScore"] = tobiiScore;


            //==

            var htmlContent = new StringBuilder();
            var html = File.ReadAllText("..\\..\\Leaderboard.template");
            if (!_settings.TryGetValue(_leaderBoardId + "_name", out var listName))
                listName = leaderboard.Players.Single(p => p.Id == _leaderBoardId.ToString()).Name;
            html = html.Replace("$(TITLE)", listName);
            html = html.Replace("$(GENDATE)", DateTime.Now.ToString());
            html = html.Replace("$(DATADATE)", File.GetLastWriteTime(_jsonFileName).ToString());

            htmlContent.AppendLine("<div class=\"w3-container\">");
            htmlContent.AppendLine("<div id=\"tabmenu\" class=\"w3-bar w3-black\">");
            var first = true;
            htmlContent.Append($"<div>Leaderboard for {listName} {_year}");
            if (_excludeZero)
                htmlContent.Append(", inactive players removed (<a href=\"" + _htmlFileName.Replace(".html", "_0.html") + "\">add them</a>)");
            else
                htmlContent.Append($", <a href=\"https://adventofcode.com/{_year}/leaderboard/private/view/{_leaderBoardId}\">AoC-style</a> (<a href=\"{_htmlFileName.Replace("_0.html", ".html")}\">remove inactive players</a>)");
            htmlContent.Append("</div>");

            foreach (var k in tabs.Keys.OrderBy(x => x))
            {
                var tabName = k.Split(new[] { '-' }, 2).Last();
                var red = first ? " w3-red" : "";
                htmlContent.AppendLine(
                    $"    <button id=\"{k}_btn\" class=\"w3-bar-item w3-button tablink{red}\" onclick=\"openTab(event.currentTarget,'{k}')\">{tabName}</button>");
                first = false;
            }

            htmlContent.AppendLine("<div align='right'>");
            foreach (var year in _years.OrderBy(y => y))
                if (year != _year)
                    htmlContent.Append($"<a href='leaderboard_{_leaderBoardId}_{year}{_x_suffix}.html'>{year}</a> ");
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
            var identical = false;
            if (File.Exists(_htmlFileName))
            {
                var oldHtml = File.ReadAllText(_htmlFileName, Encoding.UTF8);
                var part1 = oldHtml.Split(new[] { "<!-- timestamps -->" }, StringSplitOptions.RemoveEmptyEntries)[0];

                var html2 = html.Substring(0, Math.Min(html.Length, part1.Length));
                identical = part1 != html2;
            }


            if (!File.Exists(_htmlFileName) || identical)
            {
                File.WriteAllText(_htmlFileName, html, Encoding.UTF8);
                return true;
            }

            return false;

        }

        private string EndOfRow(Player player)
        {
            return Cell(player.Name) + "</tr>";
        }

        private string AddStartOfRowAndNameCell(Player p, int positionOverride = -1)
        {
            var parts = p.Props.Split(new[] { ',' });
            var res = "<tr class=\"item\">";
            var pos = (positionOverride != -1 ? positionOverride : p.CurrentPosition);
            res += Cell(pos + ". " + p.Name, pos);
            for (int i = 0; i < _metacolumns.Count; i++)
                res += Cell(parts.Length > i ? parts[i].Trim() : "");
            res += CellWithAlt(p.LocalScore, p.PendingPoints == 0 ? "" : ("Max: " + (p.LocalScore + p.PendingPoints)));
            res += Cell(p.GlobalScore);
            res += Cell(p.Stars);
            res += Cell(p.AccumulatedTobiiScoreTotal);
            return res;
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

        private bool DownloadIfOld(TimeSpan interval, bool forceLoad)
        {
            var timestamp = File.GetLastWriteTime(_jsonFileName);
            var age = DateTime.Now - timestamp;
            if (age > interval || forceLoad)
            {
                Log($"Downloading new data for {_settings[_leaderBoardId + "_name"]}/{_year} ({_leaderBoardId}), ForceLoad: {forceLoad}, ExclZero: {_excludeZero}, ExcludeUsers: {_handleExcludes}");
                // Create Target
                var url = $"https://adventofcode.com/{_year}/leaderboard/private/view/{_leaderBoardId}.json";
                try
                {
                    var s = DownloadFromURL(url);
                    if (File.Exists(_jsonFileName) && File.ReadAllText(_jsonFileName) == s)
                        return false;
                    if (!string.IsNullOrEmpty(s))
                        File.WriteAllText(_jsonFileName, s);
                    else
                        Log("Session cookie expired???");
                    return true;
                }
                catch (Exception e)
                {
                    Log("Failed to download:" + e.Message);
                }
            }

            return false;
        }

        private string DownloadFromURL(string url)
        {
            var cookies = new CookieContainer();

            // Create a WebRequest object and assign it a cookie container and make them think your Mozilla ;)
            cookies.Add(new Cookie("session", _settings["cookie_" + _leaderBoardId], "/", ".adventofcode.com"));
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Accept = "*/*";
            webRequest.AllowAutoRedirect = false;
            webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.0.3705)";
            webRequest.CookieContainer = cookies;
            //            webRequest.ContentType = "text/xml";
            webRequest.Credentials = null;

            // Grab the response from the server for the current WebRequest
            using (var webResponse = webRequest.GetResponse())
            using (var stream = webResponse.GetResponseStream())
            using (var tr = new StreamReader(stream))
            {
                return tr.ReadToEnd();
            }
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
            foreach (var p in leaderboard.OrderedPlayers)
                if (p.TotalScore > 0)
                    chartScript.AppendLine($"  data.addColumn('number', '{p.Name}');");

            chartScript.AppendLine("  data.addRows([");
            return chartScript;
        }

        private void DeriveMoreStats(LeaderBoard leaderboard)
        {
            var bestTime = new TimeSpan[leaderboard.HighestDay][];
            var lastStar = new Dictionary<Player, long>();
            var playerCount = leaderboard.Players.Count;
            if (_excludeZero)
                playerCount = leaderboard.Players.Count(p => p.Stars > 0);
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

                            var lastTime = day == 0 ? TimeSpan.Zero : player.AccumulatedTimeToComplete[day - 1][1];
                            if (lastTime.HasValue)
                                player.AccumulatedTimeToComplete[day][star] = lastTime + timeSpan;
                            if (bestTime[day][star] == TimeSpan.Zero || timeSpan < bestTime[day][star])
                                bestTime[day][star] = timeSpan;
                        }
                        else
                            player.PendingPoints += playerCount - leaderboard.StarsAwarded[day][star];
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
                            var index = orderedPlayers.IndexOf(player);
                            // handle ties
                            if (index > 0 && player.unixCompletionTime[day][star] == orderedPlayers[index - 1].unixCompletionTime[day][star])
                                player.PositionForStar[day][star] = orderedPlayers[index - 1].PositionForStar[day][star];
                            else
                                player.PositionForStar[day][star] = index;

                            if (!ExcludeDay(day)) // points day 6 were recalled 
                            {
                                player.TotalScore += playerCount - player.PositionForStar[day][star];
                                player.AccumulatedTobiiScoreTotal += player.PositionForStar[day][star];
                            }
                            player.OffsetFromWinner[day][star] = player.TimeToComplete[day][star] - bestTime[day][star];
                            lastStar[player] = player.unixCompletionTime[day][star];
                        }

                        player.AccumulatedScore[day][star] = player.TotalScore;
                        if (player.TotalScore > leaderboard.TopScorePerDay[day][star])
                            leaderboard.TopScorePerDay[day][star] = player.TotalScore;

                        player.AccumulatedTobiiScore[day][star] = player.AccumulatedTobiiScoreTotal;

                        player.LocalScore = player.TotalScore;

                    }
                }

                for (int star = 0; star < 2; star++)
                {
                    var orderedPlayers = leaderboard.Players.Where(p => p.AccumulatedScore[day][star] != 0)
                        .OrderByDescending(p => p.AccumulatedScore[day][star]).ToList();
                    foreach (var player in leaderboard.Players)
                    {
                        var index = orderedPlayers.IndexOf(player);
                        // handle ties
                        if (index > 0 && player.AccumulatedScore[day][star] == orderedPlayers[index - 1].AccumulatedScore[day][star])
                            player.AccumulatedPosition[day][star] = orderedPlayers[index - 1].AccumulatedPosition[day][star];
                        else
                            player.AccumulatedPosition[day][star] = index;
                    }
                }

                var players = leaderboard.OrderedPlayers.ToList();
                for (int i = 0; i < players.Count; i++)
                    players[i].CurrentPosition = i + 1;
            }
        }

        private bool ExcludeDay(int day)
        {
            // Because of a bug in the day 6 puzzle that made it unsolvable for some users until about two hours after unlock, day 6 is worth no points.

            if (_year == 2018 && day == 5) return true;
            if (_settings.TryGetValue($"excludeday_{_year}_{day}", out var ids))
            {
                if (ids.Split(',').Select(int.Parse).Contains(_leaderBoardId))
                    return true;
            }

            return false;

        }

        private LeaderBoard ParseJson()
        {
            var highestDay = 0;
            var players = new List<Player>();

            var text = File.ReadAllText(_jsonFileName);
            if (text.StartsWith("<"))
            {
                File.Delete(_jsonFileName);
                Log($"Logfile contained html data. deleting it");
                return null;
            }

            JObject json;
            try
            {
                json = (JObject) JsonConvert.DeserializeObject(text);
            }
            catch (Exception e)
            {
                File.Delete(_jsonFileName);
                Log($"Failed to parse json: "+e.Message);
                return null;
            }

            var jmembers = json.Property("members").First();
            foreach (JProperty jmember in jmembers)
            {
                var jmemberdata = ((JObject)jmember.Value);
                var id = jmember.Name;
                var name = GetStr(jmemberdata, "name");

                if (name == null)
                    name = "anon " + id;

                if (_settings.ContainsKey(id + "_realname"))
                    name = _settings[id + "_realname"] + " (" + name + ")";


                if (_handleExcludes && _settings.ContainsKey($"exclude_{_leaderBoardId}_{id}_{_year}"))
                {
                    Console.WriteLine($"Excluded {name} from {_year}");
                    continue;
                }
                var player = new Player
                {
                    Id = id,
                    GlobalScore = GetInt(jmemberdata, "global_score"),
                    LocalScore = GetInt(jmemberdata, "local_score"),
                    Stars = GetInt(jmemberdata, "stars"),
                    LastStar = GetLong(jmemberdata, "last_star_ts"),
                    Name = name
                };

                var propsKey = player.Id + "_" + _leaderBoardId;
                if (!_settings.TryGetValue(propsKey, out var props))
                {
                    File.AppendAllLines("..\\..\\settings.txt", new[]
                    {
                        "// " + player.Name,
                        propsKey + "=?, ?"
                    });
                    _settings[propsKey] = "?, ?";
                    player.Props = "";
                }
                else
                    player.Props = props;

                var x = jmemberdata.Property("completion_day_level");
                foreach (JProperty daydata in x.First)
                {
                    var day = int.Parse(daydata.Name);
                    highestDay = Math.Max(highestDay, day);
                    foreach (JProperty stardata in daydata.Value)
                    {
                        var starnum = int.Parse(stardata.Name);
                        var starts = GetLong((JObject)stardata.Value, "get_star_ts");
                        player.unixCompletionTime[day - 1][starnum - 1] = starts;

                    }
                }
                players.Add(player);
            }

            return new LeaderBoard(players, highestDay);
        }

        private static string Cell(string value, int sort, bool alignRight = false, string htmlClass = "", string alt = "")
        {
            var align = alignRight ? " align='right'" : "";
            htmlClass = htmlClass == "" ? "" : " class='" + htmlClass + "'";
            var s = $"<td sort='{sort}'{align}{htmlClass}>";
            if (alt != string.Empty)
                s += $"<span title=\"{alt}\">";
            s += $"{value}";
            if (alt != string.Empty)
                s += $"</span>";
            s += "</td>";
            return s;
        }
        private static string Cell(string value)
        {
            var sort = 0;
            if (value.Length > 0)
                sort = (byte)value[0];
            return Cell(value, sort);
        }

        private static string CellWithAlt(int value, string alt)
        {
            return Cell(value.ToString(), value, true, "", alt);
        }
        private static string Cell(int value, string htmlClass = "")
        {
            return Cell(value.ToString(), value, true, htmlClass);
        }
        private static string Cell(TimeSpan value, string htmlClass, string alt = "")
        {
            return Cell(value.ToString(), (int)value.TotalSeconds, true, htmlClass, alt);
        }
        private static string EmptyCell(int sort = int.MaxValue)
        {
            return Cell("", sort);
        }

        private int tableid = 0;
        private readonly Dictionary<string, string> _settings;
        private List<string> _metacolumns;
        private bool _handleExcludes;
        private string _x_suffix;
        private bool _excludeZero;

        private string TableHeader(int colPos, int tableid, string header, string alt = "", string alignment="left")
        {
            var content = header;
            if (alt != "")
                content = $"<span title=\"{alt}\">{content}</span>";
            return
                $"<th onclick=\"sortTable({colPos}, 'table_{tableid}')\" align='{alignment}' class='sortable'>{content}</th>";
        }

        private StringBuilder InitLog(string name, int highestDay, bool colForStar = true, string description = "")
        {
            var starCols = colForStar ? 2 : 1;
            //            var randomid = _r.Next();
            tableid++;
            var colPos = 0;
            var log = new StringBuilder($"<h1>{name}</h1><div style=\"overflow-x:auto;\"> <table id=\"table_{tableid}\"><tr>");
            log.AppendLine(TableHeader(colPos++, tableid, name + (string.IsNullOrEmpty(description)?"":$"<br><small>{description}</small>")));
            foreach (var s in _metacolumns)
                log.AppendLine(TableHeader(colPos++, tableid, s));
            log.AppendLine(TableHeader(colPos++, tableid, "L", "Local score", "right"));
            log.AppendLine(TableHeader(colPos++, tableid, "G", "Global score", "right"));
            log.AppendLine(TableHeader(colPos++, tableid, "S", "Stars", "right"));
            log.AppendLine(TableHeader(colPos++, tableid, "T", "Tobii Score, lower is better. Winner for each star gets 0 points ", "right"));
            for (int day = 0; day < highestDay; day++)
                for (int star = 0; star < starCols; star++)
                {
                    var stars = new string('*', star + 1);
                    string cellContent = "";
                    if (ExcludeDay(day))
                        cellContent += "<p style=\"color:red\">";
                    cellContent += $"Day {day + 1}<br>{stars}";
                    if (ExcludeDay(day))
                        cellContent += "</p>";
                    log.AppendLine($"<th onclick=\"sortTable({colPos++}, 'table_{tableid}')\" align='right' class='sortable'>{cellContent}</th>");
                }

            log.AppendLine($"<th onclick=\"sortTable({colPos++}, 'table_{tableid}')\" align='left' class='sortable'>Player Name</th>");
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


        private static int GetInt(JObject jmemberdata, string propname)
        {
            return (int)GetLong(jmemberdata, propname);
        }

        private static long GetLong(JObject jmemberdata, string propname)
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

        public void GenerateReport(int leaderboardid, int[] years, bool handleExcludes, bool forceLoad)
        {
            foreach (var year in years)
            {
                _excludeZero = true;
                GenerateReport(leaderboardid, year, years, handleExcludes, forceLoad);
                _excludeZero = false;
                GenerateReport(leaderboardid, year, years, handleExcludes, false);
                forceLoad = false;
            }

        }

    }
}