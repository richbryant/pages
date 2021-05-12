using System;
using pages.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace pages.Views
{
    public partial class PrecisView
    {
        [Inject]
        public  HttpClient Http { get; set;}
        public List<Precis> Articles { get; set; } = new List<Precis>();

        protected override async Task OnInitializedAsync()
        {
            var articles = await Http.GetFromJsonAsync<Precis[]>("data/articledata.json");
            Articles.AddRange(articles ?? Array.Empty<Precis>());
        }
    }
}
