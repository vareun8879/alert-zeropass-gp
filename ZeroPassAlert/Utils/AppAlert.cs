using System;
using System.ComponentModel;

namespace ZeroPassAlert.Utils
{
    public class AppAlert : INotifyPropertyChanged
    {
        // 싱글톤 인스턴스
        private static readonly AppAlert _instance = new AppAlert();
        public static AppAlert Instance => _instance;

        private long _todayVisitorCount;

        private string _todayDateText;

        public long TodayVisitorCount
        {
            get => _todayVisitorCount;
            set
            {
                if (_todayVisitorCount != value)
                {
                    _todayVisitorCount = value;
                    OnPropertyChanged(nameof(TodayVisitorCount));
                }
            }
        }

        public string TodayDateText
        {
            get => _todayDateText;
            set
            {
                if (_todayDateText != value)
                {
                    _todayDateText = value;
                    OnPropertyChanged(nameof(TodayDateText));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}