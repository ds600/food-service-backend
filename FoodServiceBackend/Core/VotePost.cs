using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Core
{
    public class VotePost
    {
        public string UserId { get; set; } = string.Empty;
        public string VoteResult { get; set; } = string.Empty;
    }
}
