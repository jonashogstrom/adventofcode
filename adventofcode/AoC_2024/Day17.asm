Register A: 22817223
Register B: 0
Register C: 0


Program: 
2,4 B = A mod 8
1,2 B = B xor 2 (flip bit 2)  
7,5 C = A / 2^B
4,5 B = B xor C
0,3 A = A / 8
1,7 B = B xor 7 (flip bit 1-3)
5,5 out B mod 8 
3,0 jnz A => 0 

B = tre lägsta bitarna i A (0-7)
B = flippa bit 2
C = 


var b = a % 8;
b = b ^ 2;
var c = a >> b; // (int)Math.Truncate(a / Math.Pow(2, b));
b = b ^ c;
a = a / 8;
b = b ^ 7;
_output.Add(b % 8);

