using System;

namespace Arma3Event.Controllers.Api
{
    public class ApiEvent
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Image { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string DetailsHref { get; set; }

        public string SubscribeHref { get; set; }

        public string[] AdditionalLinks { get; set; }
    }
}