using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FractalsWpf
{
    public sealed partial class SetRegionDialog : INotifyPropertyChanged
    {
        private double _bottomLeftX = -0.22d;
        private double _bottomLeftY = -0.70d;
        private double _topRightX = -0.21d;
        private double _topRightY = -0.69d;

        public SetRegionDialog()
        {
            InitializeComponent();

            DataContext = this;
        }

        public double BottomLeftX
        {
            get => _bottomLeftX;
            set
            {
                _bottomLeftX = value;
                OnPropertyChanged();
            }
        }

        public double BottomLeftY
        {
            get => _bottomLeftY;
            set
            {
                _bottomLeftY = value;
                OnPropertyChanged();
            }
        }

        public double TopRightX
        {
            get => _topRightX;
            set
            {
                _topRightX = value;
                OnPropertyChanged();
            }
        }

        public double TopRightY
        {
            get => _topRightY;
            set
            {
                _topRightY = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}