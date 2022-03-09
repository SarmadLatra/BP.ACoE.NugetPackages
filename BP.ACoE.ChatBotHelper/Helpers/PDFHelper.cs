using System.Text.RegularExpressions;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace BP.ACoE.ChatBotHelper.Helpers
{
    public class PdfHelper
    {
        public static byte[] GeneratePdf(string headerImageFullName, string footerImageFullName, List<ChatTextComponent> paragraphs)
        {
            using var memoryStream = new MemoryStream();
            var doc = new ChatTranscriptDocument(headerImageFullName, footerImageFullName, paragraphs);
            doc.GeneratePdf(memoryStream);
            return memoryStream.ToArray();
        }

        public const string Link = "__LINK__";
    }

    public class ChatTranscriptDocument : IDocument
    {
        private readonly string _headerImagePath;
        private readonly string _footerImagePath;
        private readonly List<ChatTextComponent> _paragraphs;
        public ChatTranscriptDocument(string headerImageFullName, string footerImageFullName, List<ChatTextComponent> paragraphs)
        {
            _headerImagePath = headerImageFullName;
            _footerImagePath = footerImageFullName;
            _paragraphs = paragraphs;
        }
        public void Compose(IDocumentContainer container)
        {
            container
            .Page(page =>
            {
                page.Margin(0);
                page.DefaultTextStyle(TextStyle.Default.Size(12).FontType("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(stack =>
                {
                    stack.Item().Image(_headerImagePath);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(stack =>
                {
                    stack.Item().Image(_footerImagePath);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            const float padding = 25;

            container.PaddingVertical(40)
                .Column(stack =>
            {
                stack.Spacing(3);
                foreach (var text in _paragraphs)
                {
                    stack.Item().PaddingLeft(padding).PaddingRight(padding).Component(text);
                }
            });
        }
    }

    public class ChatTextComponent : IComponent
    {
        private readonly string _userName;
        private readonly string _message;
        private readonly ChatTextComponentProperties _textProps;
        private const string LinkTitleRegex = @"\[(.*?)\]";
        private const string LinkTargetRegex = @"\((.*?)\)";
        private const string LinksRegex = @"\[.*?\]\(.*?\)";

        public ChatTextComponent(string userName, string message, ChatTextComponentProperties textProps)
        {
            _userName = userName;
            _message = message;
            _textProps = textProps;
        }

        public void Compose(IContainer container)
        {
            var allLinks = Regex.Matches(this._message, LinksRegex);
            container.EnsureSpace().Text(mainText =>
            {
                if (!string.IsNullOrWhiteSpace(this._userName))
                    mainText.Span($"{this._userName} : ",
                                this._textProps is ChatTextComponentProperties.BoldUserName or ChatTextComponentProperties.BoldContent
                                ? TextStyle.Default.Bold()
                                : TextStyle.Default);




                if (allLinks.Count <= 0)
                {
                    mainText.Span(this._message,
                               this._textProps is ChatTextComponentProperties.BoldMessage or ChatTextComponentProperties.BoldContent
                               ? TextStyle.Default.Bold()
                               : TextStyle.Default);
                }
                else
                {
                    SetContentsWithHyperlink(mainText, this._message, allLinks, this._textProps);
                }
            });
        }

        private void SetContentsWithHyperlink(TextDescriptor mainText, string content, MatchCollection allLinks, ChatTextComponentProperties textProps)
        {
            const string placeholder = $"{PdfHelper.Link}";

            var modifiedContent = Regex.Replace(content, LinksRegex, placeholder);
            var brokenContent = modifiedContent.Split(PdfHelper.Link);

            var minLength = Math.Min(allLinks.Count, brokenContent.Length);


            for (var i = 0; i < minLength; i++)
            {
                var linkTarget = Regex.Match(allLinks[i].Value, LinkTargetRegex).Value.Replace("(", string.Empty).Replace(")", string.Empty);
                var linkTitle = Regex.Match(allLinks[i].Value, LinkTitleRegex).Value.Replace("[", string.Empty).Replace("]", string.Empty);

                mainText.Span(brokenContent[i],
                                          textProps is ChatTextComponentProperties.BoldMessage or ChatTextComponentProperties.BoldContent
                                          ? TextStyle.Default.Bold()
                                          : TextStyle.Default);

                mainText.ExternalLocation(linkTitle, linkTarget, TextStyle.Default.Color("#0000FF").Underline());
            }

            foreach (var t in (new ArraySegment<string>(brokenContent, minLength, brokenContent.Length - minLength)))
            {
                mainText.Span(t,
                           (textProps == ChatTextComponentProperties.BoldMessage || this._textProps == ChatTextComponentProperties.BoldContent)
                           ? TextStyle.Default.Bold()
                           : TextStyle.Default);
            }

        }
    }

    public enum ChatTextComponentProperties
    {
        BoldUserName,
        BoldMessage,
        BoldContent,
        None
    }
}
