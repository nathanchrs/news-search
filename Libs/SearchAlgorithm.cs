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
                        int lo = last[text[i]];
                        i = i + m - Math.Min(j, 1 + lo);
                        j = m - 1;
                    }
                } while (i <= n - 1);

                return -1;
            }
        }

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
