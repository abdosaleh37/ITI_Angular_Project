using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.DAL.Entities
{
    public class ProductReview
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime Date { get; set; }
        public string ReviewerName { get; set; } = null!;
        public string ReviewerEmail { get; set; } = null!;

        public Product Product { get; set; } = null!;
    }
}
