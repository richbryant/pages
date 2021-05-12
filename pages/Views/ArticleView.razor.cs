using System.Net.Http;
using System.Threading.Tasks;
using Markdig;
using Markdig.Prism;
using Microsoft.AspNetCore.Components;

namespace pages.Views
{
    public partial class ArticleView
    {
        private string _content = string.Empty;

        [Inject]
        public  HttpClient Http { get; set;}

        [Parameter]
        public string ArticleFile { get; set; } = "compelling-example";

        public MarkupString Article => new(_content);
        
        protected override async Task OnInitializedAsync()
        {
            var articleRaw = await Http.GetStringAsync($"data/articles/{ArticleFile}.md");
            _content = Markdown.ToHtml(articleRaw, new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePrism()
                .Build());
        }
    }
}
