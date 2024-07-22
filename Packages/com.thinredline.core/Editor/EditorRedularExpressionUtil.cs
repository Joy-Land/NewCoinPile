using System;
using System.Text.RegularExpressions;

namespace ThinRL.Core.Editor
{
    public class EditorRedularExpressionUtil
    {
        public enum ERegularExpressionType
        {
            CSharpStandard = -1,//标准C#正则表达式
            FullWord = 0,//全字匹配
            Begin = 1,//e.g. pattern:@"ABC[.]*" match:"ABC00001","ABC"
            End = 2,//e.g. pattern:@"[.]*XYZ" match:"00001XYZ","XYZ"
            BeginAndEnd = 3,//e.g. pattern:@"ABC[.]*XYZ" match:"ABC00001XYZ","ABCXYZ"
        }

        public static int GetPatternComponentCount(ERegularExpressionType t)
        {
            int c = 1;
            switch(t)
            {
                case ERegularExpressionType.CSharpStandard:
                case ERegularExpressionType.FullWord:
                case ERegularExpressionType.Begin:
                case ERegularExpressionType.End:
                    {
                        c = 1;
                    }
                    break;
                case ERegularExpressionType.BeginAndEnd:
                    {
                        c = 2;
                    }
                    break;
            }
            return c;
        }

        public static bool IsMatched(ERegularExpressionType t, string input, params string[] components)
        {
            bool matched = false;
            switch (t)
            {
                case ERegularExpressionType.CSharpStandard:
                    {
                        if (components.Length < 1) throw new IndexOutOfRangeException();
                        matched = IsMatched_CSharpStandard(input, components[0]);
                    }
                    break;
                case ERegularExpressionType.FullWord:
                    {
                        if (components.Length < 1) throw new IndexOutOfRangeException();
                        matched = IsMatched_FullWord(input, components[0]);
                    }
                    break;
                case ERegularExpressionType.Begin:
                    {
                        if (components.Length < 1) throw new IndexOutOfRangeException();
                        matched = IsMatched_Begin(input, components[0]);
                    }
                    break;
                case ERegularExpressionType.End:
                    {
                        if (components.Length < 1) throw new IndexOutOfRangeException();
                        matched = IsMatched_End(input, components[0]);
                    }
                    break;
                case ERegularExpressionType.BeginAndEnd:
                    {
                        if (components.Length < 2) throw new IndexOutOfRangeException();
                        matched = IsMatched_BeginAndEnd(input, components[0], components[1]);
                    }
                    break;
            }
            return matched;
        }

        public static bool IsMatched_CSharpStandard(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
        public static bool IsMatched_FullWord(string input, string word)
        {
            return input.Equals(word);
        }
        public static bool IsMatched_Begin(string input, string begin)
        {
            return input.StartsWith(begin);
        }
        public static bool IsMatched_End(string input, string end)
        {
            return input.EndsWith(end);
        }
        public static bool IsMatched_BeginAndEnd(string input, string begin, string end)
        {
            string pattern = $@"^{begin}.*{end}$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
