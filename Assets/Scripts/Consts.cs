using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Consts 
{
    public static readonly char TagSplitChar = ' ';
    private static readonly string _tagRegexStr = $@"<#([A-Z]){TagSplitChar}(.+?)>";
    public  static readonly Regex TagRegex = new Regex(_tagRegexStr,RegexOptions.Singleline);
}
