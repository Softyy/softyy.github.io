using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

class Solution {

    // Complete the minimumBribes function below.
    static void minimumBribes(int[] q) {
        int overtakes = 0;
        for(int n=q.Length-1;n>=0;n--)
        {
            if(q[n] - (n+1) > 2)
            {
                Console.WriteLine("Too chaotic");
                return;
            }
            for (int m = Math.Max(0,q[n]-2); m < n; m++)
            {
                if(q[m] > q[n])
                    overtakes++;
            }
        }
        Console.WriteLine(overtakes);

    }

    static void Main(string[] args) {
        int t = Convert.ToInt32(Console.ReadLine());

        for (int tItr = 0; tItr < t; tItr++) {
            int n = Convert.ToInt32(Console.ReadLine());

            int[] q = Array.ConvertAll(Console.ReadLine().Split(' '), qTemp => Convert.ToInt32(qTemp))
            ;
            minimumBribes(q);
        }
    }
}
