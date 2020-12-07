﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameBook.Domain;
using GameBook.io;
using GameBook.ViewModel.Annotations;
using GameBook.Wpf.ViewModels.Command;

namespace GameBook.Wpf.ViewModels
{
    public class GameBookViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly string _sessionFilePath;
        private readonly IReadingSession _readingSession;
        private readonly IChooseResource _chooser;
        private readonly IReadingSessionRepository _sessionRepository;
        public ObservableCollection<ChoiceViewModel> Choices { get; }
        public ObservableCollection<VisitedParagraphsViewModel> VisitedParagraphs { get; }
        public ICommand LoadBook { get; private set; }
        private ICommand GoToParagraph { get; }
        public ICommand GoBack { get; }
        private ICommand Open { get; }
        public ICommand SaveOnClose { get; }
        
        public GameBookViewModel(IReadingSession readingSession, IChooseResource chooser, IReadingSessionRepository sessionRepository)
        {
            LoadBook = ParameterlessRelayCommand.From(() =>
            {
                var path = _chooser.ResourceIdentifier;
            });
            GoToParagraph = ParameterizedRelayCommand<ChoiceViewModel>.From(DoGoToParagraph);
            GoBack = ParameterlessRelayCommand.From(DoGoBack);
            Open = ParameterlessRelayCommand.From(DoOpen);
            SaveOnClose = ParameterlessRelayCommand.From(DoSaveOnClose);
            _readingSession = readingSession;
            _chooser = chooser;
            _sessionRepository = sessionRepository;
            _sessionFilePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.FullName + $"\\readingSession.json";
            Choices = new ObservableCollection<ChoiceViewModel>();
            VisitedParagraphs = new ObservableCollection<VisitedParagraphsViewModel>();
            OpenLastSession();
            UpdateChoices();
            UpdateVisitedParagraphs();
        }

        private void OpenLastSession()
        {
            IList<int> lastSession = _sessionRepository.Open(_readingSession.GetBookTitle(), _sessionFilePath);
            if (lastSession == null || lastSession.Count == 0) return;
            _readingSession.OpenLastSession(lastSession);
        }

        private void DoGoToParagraph(ChoiceViewModel choice)
        {
            _readingSession.GoToParagraphByChoice(choice.Destination);
            Refresh();
        }

        private void DoGoToKnownParagraph(VisitedParagraphsViewModel visitedParagraph)
        {
            _readingSession.GoToVisitedParagraph(visitedParagraph.Index);
            Refresh();
        }

        private void DoGoBack()
        {
            _readingSession.GoBackToPrevious();
            Refresh();
        }
        
        private void DoOpen()
        {
            
            using (TextReader fileStream = File.OpenText(_chooser.ResourceIdentifier))
            {

            }
        }

        private void DoSaveOnClose()
        {
            _sessionRepository.Save(_readingSession, _sessionFilePath);
        }

        private void Refresh()
        {
            UpdateChoices();
            UpdateVisitedParagraphs();
            OnPropertyChanged(nameof(Choices));
            OnPropertyChanged(nameof(CurrentParagraph));
            OnPropertyChanged(nameof(ParagraphContent));
            OnPropertyChanged(nameof(WarningMessage));
            OnPropertyChanged(nameof(VisitedParagraphs));
        }

        private void UpdateChoices()
        {
            Choices.Clear();
            
            foreach (var (key, value) in _readingSession.GetParagraphChoices(_readingSession.GetCurrentParagraph()))
            {
                Choices.Add(new ChoiceViewModel(key, value, GoToParagraph));
            }
        }

        private void UpdateVisitedParagraphs()
        {
            VisitedParagraphs.Clear();
            foreach (var (key, value) in _readingSession.GetHistory())
            {
                VisitedParagraphs.Add(new VisitedParagraphsViewModel(key, value));
            }
        }

        public string BookTitle => _readingSession.GetBookTitle();

        public string CurrentParagraph => $"Paragraph {_readingSession.GetCurrentParagraph()}";

        public string ParagraphContent => _readingSession.GetParagraphContent();

        public string WarningMessage => _readingSession.WarningMessage;

        public VisitedParagraphsViewModel SelectedParagraph
        {
            set
            {
                if (value == null) return;
                DoGoToKnownParagraph(value);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChoiceViewModel
    {
        public string ChoiceText { get; }
        public int Destination { get; }
        public ICommand GoToParagraph { get; }
        public ChoiceViewModel(string text, int destination, ICommand goToParagraph)
        {
            ChoiceText = $"{text} (->{destination})";
            Destination = destination;
            GoToParagraph = goToParagraph;
        }
    }

    public class VisitedParagraphsViewModel
    {
        public string Label { get; }
        public int Index { get; }
        public VisitedParagraphsViewModel(int key, string value)
        {
            Index = key;
            Label = value;
        }
    }
}