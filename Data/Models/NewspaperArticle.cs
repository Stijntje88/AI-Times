using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Times.Data.Models
{
    public class NewspaperArticle

    {
        public int Id { get; set; }  

        public string Title { get; set; }

        public string Image1 { get; set; }

        public string Image2 { get; set; }

        public string Introduction { get; set; }

        public string MiddleSection { get; set; }

        public string Conclusion { get; set; }

        public DateTime PublishDate { get; set; }

        public string FormattedPublishDate => PublishDate.ToString("MMM dd, yyyy");

        public string Author { get; set; }
    }
}
