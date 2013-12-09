using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class GroupsViewModel
    {
        public Group Group { get; set; }
        public List<UserProfile> Members { get; set; }
    }
}