﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogProject.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string BlogUserId { get; set; }

        [Required]
        [StringLength(25, ErrorMessage = "The{0} must be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string Text { get; set; }

        //NAVIGATION PROPERTIES
        public virtual Post Post { get; set; }
        public virtual BlogUser BlogUser { get; set; }
    }
}
