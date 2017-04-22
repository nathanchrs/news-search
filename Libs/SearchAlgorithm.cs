using news_search.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<IEnumerable<Post>> GetAllPost(string method, string pattern)
        {
            var posts = await RssParser.ReadFeedAsync(@"http://rss.detik.com/index.php/detikcom");
            List<Post> exceptionPost = new List<Post>();

            foreach (var post in posts)
            {
                if (method.Equals("kmp"))
                {
                    //post.Content = await HtmlParser.ParsingDetik(post.Link);
                    if (KmpMatch(post.Title, pattern) == -1)
                    {
                        exceptionPost.Add(post);
                    }
                }
                else if (method.Equals("boyer-moore"))
                {
                    //post.Content = await HtmlParser.ParsingDetik(post.Link);
                    if (BmMatch(post.Title, pattern) == -1)
                    {
                        exceptionPost.Add(post);
                    }
                }
                else
                {
                    // regex implementation
                }
            }

            return posts.Except(exceptionPost);
        }
    }
}
