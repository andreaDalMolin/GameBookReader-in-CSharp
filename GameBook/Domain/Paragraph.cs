﻿using System.Collections.Generic;

namespace GameBook.Domain
{
    public class Paragraph
    {
        public Paragraph(int index, string text, params Choice[] newChoices)
        {
            Index = index;
            Text = text;
            foreach (var choice in newChoices)
            {
                Choices.Add(choice);
            }
        }

        public virtual string Text { get; }

        public IList<Choice> Choices { get; } = new List<Choice>();

        public virtual int Index { get; }

        public virtual bool IsTerminal() => Choices.Count == 0;

        public int GetChoiceDestination(in int choiceIndex)
        {
            return choiceIndex >= 0 && choiceIndex < Choices.Count ? Choices[choiceIndex].DestParagraph : -1;
        }

        public virtual IList<string> GetChoices()
        {
            IList<string> choicesLabels = new List<string>();
            foreach (var choice in Choices)
            {
                choicesLabels.Add(choice.Text);
            }

            return choicesLabels;
        }

        public virtual string GetLabel()
        {
            var label = "";
            var firstWords = Text.Split(' ');
            for (var i = 0; i < 4; i++)
            {
                if (i < firstWords.Length)
                {
                    label += firstWords[i] + " ";
                }
            }

            return $"{label}...";
        }
    }
}