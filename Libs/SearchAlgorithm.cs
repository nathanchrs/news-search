using news_search.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace news_search.Libs
{
    public class SearchAlgorithm {
        private static int KmpMatch(String text, String pattern) {
            int n = text.Length;
            int m = pattern.Length;
            int[] fail = ComputeFail(pattern);

            int i = 0;
            int j = 0;
            while (i < n) {
                if (pattern[j] == text[i]) {
                    if (j == m - 1) {
                        return i - m + 1;
                    }
                    i++;
                    j++;
                } else if (j > 0) {
                    j = fail[j - 1];
                } else {
                    i++;
                }
            }
            return -1;
        }

        private static int[] ComputeFail(String pattern)
        {
            int[] fail = new int[pattern.Length];
            fail[0] = 0;

            int m = pattern.Length;
            int j = 0;
            int i = 1;

            while (i < m) {
                if (pattern[j] == pattern[i]) {
                    fail[i] = j + 1;
                    i++;
                    j++;
                } else if (j > 0) {
                    j = fail[j - 1];
                } else {
                    fail[i] = 0;
                    i++;
                }
            }
            return fail;
        }


        public static int[] BuildLast(String pattern)
        {
            int[] last = new int[256];
            for (int i = 0; i < 256; i++)
            {
                last[i] = -1;
            }
            for (int i = 0; i < pattern.Length; i++)
            {
                if ((pattern[i] >= 97 && pattern[i] <= 122))
                {
                    last[pattern[i]] = i;
                    last[pattern[i] - 32] = i;
                }
                else if (pattern[i] >= 65 && pattern[i] <= 90)
                {
                    last[pattern[i]] = i;
                    last[pattern[i] + 32] = i;
                }
                else
                {
                    last[pattern[i]] = i;
                }
            }
            return last;
        }
        public static int BmMatch(String text, String pattern)
        {
            int[] last = BuildLast(pattern);
            int n = text.Length;
            int m = pattern.Length;
            int i = m - 1;
            if (i > n - 1)
            {
                return -1;
            }
            int j = m - 1;
            do
            {
                if (pattern[j] >= 65 && pattern[j] <= 90)
                {
                    if ((pattern[j] == text[i]) || pattern[j] + 32 == text[i])
                    {
                        if (j == 0)
                        {
                            return i;
                        }
                        else
                        {
                            i--;
                            j--;
                        }
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(i);
                            int lastoccur = last[text[i]];
                            i = i + m - Math.Min(j, 1 + lastoccur);
                            j = m - 1;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            i--;
                            j--;
                        }
                    }
                }
                else if (pattern[j] >= 97 && pattern[j] <= 122)
                {
                    if ((pattern[j] == text[i]) || pattern[j] - 32 == text[i])
                    {
                        if (j == 0)
                        {
                            return i;
                        }
                        else
                        {
                            i--;
                            j--;
                        }
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(i);
                            int lastoccur = last[text[i]];
                            i = i + m - Math.Min(j, 1 + lastoccur);
                            j = m - 1;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            i--;
                            j--;
                        }
                    }
                }
                else
                {
                    if (pattern[j] == text[i])
                    {
                        if (j == 0)
                        {
                            return i;
                        }
                        else
                        {
                            i--;
                            j--;
                        }
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(i);
                            int lastoccur = last[(int)text[i]];
                            i = i + m - Math.Min(j, 1 + lastoccur);
                            j = m - 1;
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            i--;
                            j--;
                        }
                    }

                }
            } while (i <= n - 1);
            return -1; 
        }

        /*
        private static int[] BuildLast(String pattern) {
            int[] last = new int[128];
            for (int i = 0; i < 128; i++) {
                last[i] = -1;
            }

            for (int i = 0; i < pattern.Length; i++) {
                last[pattern[i]] = i;
            }

            return last;
        }
        
        private static void BadCharHeuristic(string str, int size, ref int[] badChar)
        {
            int i;

            for (i = 0; i < 256; i++)
                badChar[i] = -1;

            for (i = 0; i < size; i++)
                badChar[(int)str[i]] = i;
        }

        
        private static int BmMatch(String text, String pattern)
        {
            List<int> retVal = new List<int>();
            int m = pattern.Length;
            int n = text.Length;

            int[] badChar = new int[256];

            BadCharHeuristic(pattern, m, ref badChar);

            int s = 0;
            while (s <= (n - m))
            {
                int j = m - 1;

                while (j >= 0 && pattern[j] == text[s + j])
                    --j;

                if (j < 0)
                {
                    retVal.Add(s);
                    s += (s + m < n) ? m - badChar[text[s + m]] : 1;
                }
                else
                {
                    s += Math.Max(1, j - badChar[text[s + j]]);
                }
            }

            if(retVal.Count == 0)
            {
                return -1;
            } else
            {
                return retVal[0];
            }
           
        }
        

        private static int BmMatch(String text, String pattern) {
            int[] last = BuildLast(pattern);
            int n = text.Length;
            int m = pattern.Length;

            int i = m - 1;

            if (i > n - 1) {
                return -1;
            } else {
                int j = m - 1;
                do {
                    if (pattern[j] == text[i]) {
                        if (j == 0) {
                            return i;
                        } else {
                            i--;
                            j--;
                        }
                    } else {
                        try
                        {
                            int lo = last[text[i]];
                            i = i + m - Math.Min(j, 1 + lo);
                            j = m - 1;
                        } catch(IndexOutOfRangeException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        
                        
                    }
                } while (i <= n - 1);

                return -1;
            }
        }
        */

        public static int RegexMatch(String text, String pattern) {
            /*string patternRegex = string.Empty;
            String[] patternFormat = pattern.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            if (patternFormat.Length > 1)
            {
                for (int i = 0; i < patternFormat.Length; i++)
                {
                    patternRegex += patternFormat[i];
                    patternRegex += @" .* ";
                }
            }*/
            Match match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Index;
            } else {
                return -1;
            } 
        }

        private static string ExtractSentence(string text, int around) {
            string sentence = "";
            if (text != null && text != "") {
                int start = Math.Max(around - 30, 0);
                int end = Math.Min(around + 30, text.Length-1);
                sentence = text.Substring(start, end-start+1);
            }
            return sentence;
        }

        public static void FilterPosts(List<Post> posts, string pattern, string method = "kmp") {
            // Remove posts from the back to prevent index problems
            for (int idx = posts.Count - 1; idx >= 0; idx--) {
                int titleIdx = -1;
                int contentIdx = -1;
                if (posts[idx].Content == null) posts[idx].Content = "";
                if (method == "kmp") {
                    titleIdx = KmpMatch(posts[idx].Title.ToLower(), pattern.ToLower());
                    contentIdx = KmpMatch(posts[idx].Content.ToLower(), pattern.ToLower());
                } else if (method == "boyer-moore") {
                    titleIdx = BmMatch(posts[idx].Title.ToLower(), pattern.ToLower());
                    contentIdx = BmMatch(posts[idx].Content.ToLower(), pattern.ToLower());
                } else if (method == "regex") {
                    titleIdx = RegexMatch(posts[idx].Title, pattern);
                    contentIdx = RegexMatch(posts[idx].Content, pattern);
                }

                if (titleIdx != -1) {
                    posts[idx].RelevantContent = posts[idx].Title;
                } else if (contentIdx != -1) {
                    posts[idx].RelevantContent = ExtractSentence(posts[idx].Content, contentIdx);
                } else {
                    posts.RemoveAt(idx);
                }

            }
        }

    }
}
