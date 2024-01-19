﻿using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TDMD.Classes
{
    public class Lamp : INotifyPropertyChanged
    {
        private bool _status;
        private double _brightness;
        public string ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ModelID { get; set; }
        public string SWVersion { get; set; }
        public string UniqueID { get; set; }

        public bool Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public double Brightness
        {
            get { return _brightness; }
            set
            {
                _brightness = value;
                OnPropertyChanged();
            }
        }
        public double BrightnessPercentage { get; set; }
        public int Hue { get; set; }
        public int Sat { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task ToggleLamp()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                bool otherState = !Status;
                string url = $"http://192.168.12.24/api/{Communicator.userid}/lights/{ID}/state";
                string body = $"{{\"on\":{otherState.ToString().ToLower()}}}";

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Lamp {ID} turned on successfully.");
                    Status = otherState;
                }
                else
                {
                    Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        public async Task SetBrightness(double value)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = $"http://192.168.12.24/api/{Communicator.userid}/lights/{ID}/state";
                string body = $"{{\"bri\":{value}}}";

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Lamp {ID} turned on successfully.");
                    Brightness = value;
                    BrightnessPercentage = Converter.ValueToPercentage(Brightness);
                }
                else
                {
                    Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }

        public async Task SetColor(int hue, int sat)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string url = $"http://192.168.12.24/api/{Communicator.userid}/lights/{ID}/state";
                string body = $"{{\"hue\": {hue}, \"sat\": {sat}}}";

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"Lamp {ID} turned on successfully.");
                    Hue = hue;
                    Sat = sat;
                }
                else
                {
                    Debug.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }


    }
}