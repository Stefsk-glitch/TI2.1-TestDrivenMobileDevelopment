﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Eindopdracht.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private List<NSStation> _allStations;
        private List<NSStation> _nearestStations;
        private string _searchQuery;
        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshDataCommand { get; private set; }

        public MainViewModel()
        {
            SearchCommand = new Command(SearchStations);
            RefreshDataCommand = new Command(async () => await RefreshData());
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; private set; }

        public async Task RefreshData()
        {
            IsRefreshing = true;
            await MainPage.TaskFindNearestStations();
            IsRefreshing = false;
        }

        public List<NSStation> NearestStations
        {
            get => _nearestStations;
            set
            {
                _nearestStations = value;
                OnPropertyChanged();
            }
        }

        public List<NSStation> AllStations
        {
            get => _allStations;
            set
            {
                _allStations = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchStations(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                NearestStations = AllStations;
            }
            else
            {
                NearestStations = AllStations.FindAll(station => station.Name.ToLower().Contains(SearchQuery.ToLower()));
            }
        }
    }
}