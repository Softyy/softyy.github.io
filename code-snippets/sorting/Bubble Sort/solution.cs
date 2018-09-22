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

    // Complete the countSwaps function below.
    static void countSwaps(int[] a) {
        int numOfSwaps = 0;
        for(int n=1;n<a.Length;n++)
        {
            if (a[n]<a[n-1])
            {
                int temp = a[n];
                a[n] = a[n-1];
                a[n-1] = temp;
                numOfSwaps++;
                if (n-1 != 0)
                    n = n -2;
            }
            
        }
        
        Console.WriteLine($"Array is sorted in {numOfSwaps} swaps.");
        Console.WriteLine($"First Element: {a[0]}");
        Console.WriteLine($"Last Element: {a[a.Length-1]}");
    }

    static void Main(string[] args) {
        int n = Convert.ToInt32(Console.ReadLine());

        int[] a = Array.ConvertAll(Console.ReadLine().Split(' '), aTemp => Convert.ToInt32(aTemp))
        ;
        countSwaps(a);
    }
}
