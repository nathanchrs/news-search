using System.ComponentModel.DataAnnotations;

namespace news_search.Models
{
    public class Post
    {
        [Required]
        [Display(Name = "Published Date")]
        public string PublishedDate { set; get; }

        [Required]
        [Display(Name = "Description")]
        public string Description { set; get; }

        [Required]
        [Display(Name = "Link")]
        public string Link { set; get; }

        [Required]
        [Display(Name = "Title")]
        public string Title { set; get; }

        [Required]
        [Display(Name = "Content")]
        public string Content { set; get; }
    }
}
