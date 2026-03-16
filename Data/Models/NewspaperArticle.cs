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

        public string Subtitle1 { get; set; }

        public string Subtitle2 { get; set; }

        public string Subtitle3 { get; set; }

        public string Image1 { get; set; }

        public string Image2 { get; set; }

        public string Introduction { get; set; }

        public string MiddleSection { get; set; }

        public string Conclusion { get; set; }

        public string Genre { get; set; }

        public DateTime PublishDate { get; set; }

        public string FormattedPublishDate => PublishDate.ToString("MMM dd, yyyy");

        public string Author { get; set; }

        public bool Verified { get; set; } = false;

        public int? VerifiedBy { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string VerifiedText => Verified ? "✔ Verified" : "✖ Not Verified";

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.UI.Xaml.Media.SolidColorBrush VerifiedBrush => new Microsoft.UI.Xaml.Media.SolidColorBrush(Verified ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
    }
}
