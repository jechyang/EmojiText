using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Consts 
{
    private static readonly string _tagRegexStr = @"<#([A-Z]) (.+?)>";
    public  static readonly Regex TagRegex = new Regex(_tagRegexStr,RegexOptions.Singleline);
}
