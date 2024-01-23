﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using TDMD.Classes;
using TDMD.Interfaces;

namespace TDMD.ViewModels
{
    public partial class MainViewModel : ObservableObject, IViewModel
    {
        [ObservableProperty]
        private List<Lamp> _lampsList;

        private bool _isRefreshing = false;
        private string _status;
        private string _userID;

        private string userId;

        //when android phone: http://10.0.2.2:8000/
        //when windows: http://192.168.1.179/
        private string mainUrl = "http://10.0.2.2:80/api";

        public MainViewModel()
        {
            Lamps = new List<Lamp>(new List<Lamp>());

            InitializeAsync();
        }

        public string UserIDText
        {
            get { return _userID; }
            set
            {
                _userID = value;
                OnPropertyChanged();
            }
        }

        public List<Lamp> Lamps
        {
            get => _lampsList;
            set
            {
                _lampsList = value;
                OnPropertyChanged();
            }
        }

        public string ConnectionStatus
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        [RelayCommand]
        public async Task Refresh()
        {
            IsRefreshing = true;

            // Call your method to refresh data here
            await RefreshData();

            IsRefreshing = false;
        }

        [RelayCommand]
        async Task GoToLampInfoPage(Lamp lamp)
        {
            if (lamp == null)
                return;

            await Shell.Current.GoToAsync(nameof(LampInfoPage), true, new Dictionary<string, object>
            {
                {"Lamp", lamp }
            });
        }

        private async Task RefreshData()
        {
            if (userId != null)
            {
                LoadLamps();
            }
        }

        private async void InitializeAsync()
        {
            await GetUserIDAsync();

            if (userId != null)
            {
                await LoadLamps();
            }
            else
            {
                UserIDText = "error";
            }
        }

        private async Task GetUserIDAsync()
        {
            // before running the app click on the link button in the HUE emulator!!!
            if (await GetUserIdAsync() == false)
            {
                UserIDText = "No UserID. Link button > refresh app";
            }
            else
            {
                UserIDText = $"UserID: {userId}";
            }

        }

        public async Task LoadLamps()
        {
            string url = $"{mainUrl}" + "/" + userId;

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        ConnectionStatus = "Status: Connected!";
                        string jsonString = await response.Content.ReadAsStringAsync();

                        Debug.WriteLine(jsonString);

                        Lamps = ParseLights(jsonString);
                    }
                    else
                    {
                        Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private List<Lamp> ParseLights(string jsonResponse)
        {
            List<Lamp> lamps = new List<Lamp>();

            try
            {
                JObject jsonObject = JObject.Parse(jsonResponse);
                JObject lightsObject = jsonObject["lights"].ToObject<JObject>();

                foreach (var keyValuePair in lightsObject)
                {
                    string key = keyValuePair.Key;
                    JObject lightObject = keyValuePair.Value.ToObject<JObject>();

                    string id = key;
                    string name = lightObject["name"].ToString();
                    bool isOn = lightObject["state"]["on"].ToObject<bool>();
                    string type = lightObject["type"].ToString();
                    string swversion = lightObject["swversion"].ToString();
                    string uniqueid = lightObject["uniqueid"].ToString();
                    int brightness = lightObject["state"]["bri"].ToObject<int>();
                    int hue = lightObject["state"]["hue"].ToObject<int>();
                    int sat = lightObject["state"]["sat"].ToObject<int>();

                    Lamp lamp = new Lamp
                    {
                        ID = key,
                        Type = type,
                        Name = name,
                        ModelID = id,
                        SWVersion = swversion,
                        UniqueID = uniqueid,

                        Status = isOn,
                        Brightness = brightness,
                        BrightnessPercentage = ValueToPercentage(brightness),
                        Hue = hue,
                        Sat = sat
                    };

                    lamps.Add(lamp);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            return lamps;
        }

        private double ValueToPercentage(double value)
        {
            double percentage = value / 254.0 * 100.0;
            return Math.Round(percentage);
        }

        public async Task<bool> GetUserIdAsync()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = mainUrl;
                string body = "{\"devicetype\":\"my_hue_app#gertiemeneer\"}";

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    try
                    {
                        JArray jsonArray = JArray.Parse(result);
                        JObject successObject = jsonArray[0]["success"] as JObject;
                        userId = (string)successObject["username"];
                    }
                    catch
                    {
                        return false;
                    }

                    Debug.WriteLine($"User ID: {userId}");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return false;
                }
            }
        }
    }
}
