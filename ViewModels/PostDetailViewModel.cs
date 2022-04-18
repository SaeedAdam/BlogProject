using BlogProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogProject.ViewModels
{
    public class PostDetailViewModel
    {
        public Post Post { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
