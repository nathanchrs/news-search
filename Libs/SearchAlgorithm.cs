using news_search.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace news_search.Libs
{
    public class SearchAlgorithm
    {
        private static int KmpMatch(String text, String pattern)
        {
            int n = text.Length;
            int m = pattern.Length;

            int[] fail = ComputeFail(pattern);

            int i = 0;
            int j = 0;

            while (i < n)
            {
                if (pattern[j] == text[i])
                {
                    if (j == m - 1)
                    {
                        return i - m + 1;
                    }
                    i++;
                    j++;
                }
                else if (j > 0)
                {
                    j = fail[j - 1];
                }
                else
                {
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

            while (i < m)
            {
                if (pattern[j] == pattern[i])
                {
                    fail[i] = j + 1;
                    i++;
                    j++;
                }
                else if (j > 0)
                {
                    j = fail[j - 1];
                }
                else
                {
                    fail[i] = 0;
                    i++;
                }
            }
            return fail;
        }

        private static int[] BuildLast(String pattern)
        {
            int[] last = new int[128];

            for (int i = 0; i < 128; i++)
            {
                last[i] = -1;
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                last[pattern[i]] = i;
            }

            return last;
        }

        private static int BmMatch(String text, String pattern)
        {
            int[] last = BuildLast(pattern);

            int n = text.Length;
            int m = pattern.Length;

            int i = m - 1;

            if (i > n - 1)
            {
                return -1;
            }
            else
            {
                int j = m - 1;
                do
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
                        int lo = last[text[i]];
                        i = i + m - Math.Min(j, 1 + lo);
                        j = m - 1;
                    }
                } while (i <= n - 1);

                return -1;
            }
        }

        public static int RegexMatch(String text, String pattern)
        {
            String[] patternFormat = pattern.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            string patternRegex = string.Empty;

            if (patternFormat.Length > 1)
            {
                for (int i = 0; i < patternFormat.Length; i++)
                {
                    patternRegex += patternFormat[i];
                    patternRegex += " ";
                    patternRegex += @"(?<=^|\S*\s*)\S*";
                }
            }

            Match match = Regex.Match(text.ToLower(), patternRegex.ToLower(), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Index;
            }
            else
            {
                return -1;
            }
        }

        public static async Task<List<Post>> GetAllPost(string method, string pattern)                                   
        {
            var posts = await RssParser.ReadFeedAsync(@"http://rss.detik.com/index.php/detikcom");
            List<Post> allPost = new List<Post>(posts.ToList().Count);
            //List<Post> exceptionPost = new List<Post>();

            foreach (var post in posts.ToList())
            {
                Post temp = new Post();
                temp.Link = post.Link;
                temp.Description = post.Description;
                temp.PublishedDate = post.PublishedDate;
                temp.Title = post.Title;
                temp.Content = String.Empty;
                temp.Content = await HtmlParser.ParsingDetik(temp.Link);
                allPost.Add(temp);

            }

            for (int idx = allPost.Count - 1; idx >= 0; idx--)
            {
                if (method.Equals("kmp"))
                {

                    if (KmpMatch(allPost[idx].Title, pattern) == -1)
                    {
                        if (KmpMatch(allPost[idx].Content, pattern) == -1)
                        {
                            //exceptionPost.Add(post);
                            //allPost.Remove(post);
                            allPost.RemoveAt(idx);
                        }
                    }
                }
                else if (method.Equals("boyer-moore"))
                {

                    if (BmMatch(allPost[idx].Title, pattern) == -1)
                    {
                        if (BmMatch(allPost[idx].Content, pattern) == -1)
                        {
                            //exceptionPost.Add(post);
                            //allPost.Remove(post);
                            allPost.RemoveAt(idx);
                        }
                    }

                }
                else
                {

                    if (RegexMatch(allPost[idx].Title, pattern) == -1)
                    {
                        if (RegexMatch(allPost[idx].Content, pattern) == -1)
                        {
                            //allPost.Remove(post);
                            //exceptionPost.Add(post);
                            allPost.RemoveAt(idx);
                        }
                    }

                }
            }

            //return allPost.Remove(exceptionPost);
            return allPost;
        }
    }
}
