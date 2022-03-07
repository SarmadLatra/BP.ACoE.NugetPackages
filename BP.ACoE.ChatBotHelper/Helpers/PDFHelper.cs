using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace BPMeAUChatBot.API.Helpers
{
    public class PDFHelper
    {
        public static byte[] GeneratePdf(string headerImageFullName, string footerImageFullName, List<ChatTextComponent> paragraphs)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                ChatTranscriptDocument doc = new ChatTranscriptDocument(headerImageFullName, footerImageFullName, paragraphs);
                doc.GeneratePdf(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public const string LINK = "__LINK__";
    }

    public class ChatTranscriptDocument : IDocument
    {
        string _headerImagePath = string.Empty;
        string _footerImagePath = string.Empty;
        List<ChatTextComponent> _paragraphs = new List<ChatTextComponent>();
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

        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Item().Image(_headerImagePath);
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Item().Image(_footerImagePath);
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            string linkTitle = string.Empty;
            string linkTarget = string.Empty;

            float padding = 25;

            container.PaddingVertical(40).Stack(stack =>
            {
                stack.Spacing(3);
                foreach (ChatTextComponent text in _paragraphs)
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
        private readonly string linkTitleRegex = @"\[(.*?)\]";
        private readonly string linkTargetRegex = @"\((.*?)\)";
        private readonly string linksRegex = @"\[.*?\]\(.*?\)";

        public ChatTextComponent(string userName, string message, ChatTextComponentProperties textProps)
        {
            _userName = userName;
            _message = message;
            _textProps = textProps;
        }

        public void Compose(IContainer container)
        {
            MatchCollection allLinks = Regex.Matches(this._message, linksRegex);
            container.EnsureSpace().Text(mainText =>
            {
                if (!string.IsNullOrWhiteSpace(this._userName))
                    mainText.Span($"{this._userName} : ",
                                (this._textProps == ChatTextComponentProperties.BoldUserName || this._textProps == ChatTextComponentProperties.BoldContent)
                                ? TextStyle.Default.Bold()
                                : TextStyle.Default);




                if (allLinks.Count <= 0)
                {
                    mainText.Span(this._message,
                               (this._textProps == ChatTextComponentProperties.BoldMessage || this._textProps == ChatTextComponentProperties.BoldContent)
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
            string placeholder = $"{PDFHelper.LINK}";

            var modifiedContent = Regex.Replace(content, linksRegex, placeholder);
            var brokenContent = modifiedContent.Split(PDFHelper.LINK);

            var minLength = Math.Min(allLinks.Count, brokenContent.Length);


            for (int i = 0; i < minLength; i++)
            {
                var linkTarget = Regex.Match(allLinks[i].Value, linkTargetRegex).Value.Replace("(", string.Empty).Replace(")", string.Empty);
                var linkTitle = Regex.Match(allLinks[i].Value, linkTitleRegex).Value.Replace("[", string.Empty).Replace("]", string.Empty);

                mainText.Span(brokenContent[i],
                                          (textProps == ChatTextComponentProperties.BoldMessage || textProps == ChatTextComponentProperties.BoldContent)
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
