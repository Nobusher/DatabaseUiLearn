
using DbConnectionService.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReactiveUI;
using System.Reactive;
using DatabaseUiApp.Views;
namespace DatabaseUiApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Film>? Films { get; set; } = new();

        public ObservableCollection<Director>?
            Directors { get; set; } = new();

        private bool _isLoading = false;

        public ReactiveCommand<Unit, Unit>
            OpenAddDirectorCommand { get; set;}

        public ReactiveCommand<Unit,Unit> 
            OpenAddFilmCommand { get; set;}

        private Film? _selectedFilm;

        public Film? SelectedFilm 
        {
            get => _selectedFilm;
            set => 
                this.RaiseAndSetIfChanged
                (ref _selectedFilm, value);
        }
        public bool IsLoading 
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }
        public MainViewModel()
        {
            OpenAddFilmCommand
                = ReactiveCommand.Create(OpenAddFilm);
           _ = LoadFilms();

            OpenAddDirectorCommand 
                = ReactiveCommand.Create(OpenAddDirector);
        }
        private void OpenAddFilm() 
        {
            var window = new AddFilmWindow();
            window.DataContext = new AddFilmViewModel(Films!);
            window.Show();
        }
        private void OpenAddDirector()
        {
            var window = new AddDirectorWindow();
            window.DataContext 
                = new AddDirectorViewModel(Directors!);
            window.Show();

        }
        private async Task LoadFilms() 
        {
            IsLoading = true;
            
            using var db = new CinemaContext();

            await db.Database.OpenConnectionAsync();
            await Task.Delay(3500);
            var films = await db.Films
                .Include(f=>f.Director)
                .Include(f=>f.Genre)
                .ToListAsync();
            var directors = await db.Directors
                .ToListAsync();
            foreach (var director in directors) 
            {
                Directors?.Add(director);
            }
            foreach (var film in films) 
            {
                Films?.Add(film);
            }
            IsLoading = false;
        }
    }
    public class AddFilmViewModel : ViewModelBase 
    {
        private string _title = "";
        public string Title 
        {
            get => _title;
            set => this.RaiseAndSetIfChanged
                (ref _title, value);
        }
        private int _year;
        public int Year
        {
            get => _year;
            set => this.RaiseAndSetIfChanged
                (ref _year, value);
        }
        private int _duration;
        public int Duration
        {
            get=> _duration;
            set=> this.RaiseAndSetIfChanged 
                (ref _duration, value);
        }

        private Director? _selectedDirector;
        public Director? SelectedDirector
        {
            get => _selectedDirector;
            set => this.RaiseAndSetIfChanged 
                (ref _selectedDirector, value);
        }
        private Genre? _selectedGenre;
        public Genre? SelectedGenre
        {
            get => _selectedGenre;
            set => this.RaiseAndSetIfChanged 
                (ref _selectedGenre, value);
        }
        public ObservableCollection<Director> Directors
        { get; } = new();
        public ObservableCollection<Genre> Genres
        { get; } = new();
        private readonly ObservableCollection<Film> _films;
        public ReactiveCommand<Unit,Unit> SaveCommand {  get; }
        public AddFilmViewModel(ObservableCollection<Film> films) 
        {
            _films = films;
           SaveCommand = ReactiveCommand
                .CreateFromTask(SaveAsync);
            _ = LoadDataAsync();
        }
        private async Task LoadDataAsync() 
        {
            using var db = new CinemaContext();
            var directors = await db.Directors.ToListAsync();
            var genres = await db.Genres.ToListAsync();

            foreach (var d in directors) Directors.Add(d);
            foreach (var g in genres) Genres.Add(g); 
        }
        private async Task SaveAsync() 
        {
            if(SelectedDirector == null
                ||SelectedGenre == null) return;
            using var db = new CinemaContext();
            db.Films.Add(new Film 
            {
                Title = Title,
                Year = Year,
                Duration = Duration,
                DirectorId = SelectedDirector.Id,
                GenreId = SelectedGenre.Id,
            });
            await db.SaveChangesAsync();
        }
    }
    public class AddDirectorViewModel : ViewModelBase 
    {
        private readonly ObservableCollection<Director> _directors;

        private string _name = "";
        public string Name
        {
            get => _name;
            set=>this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string _country = "";
        public string Country
        {
            get => _country;
            set => this.RaiseAndSetIfChanged(ref _country, value);
        }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public AddDirectorViewModel
            (ObservableCollection<Director> directors)
        {
            _directors = directors;
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        }

        private async Task SaveAsync() 
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            using var db = new CinemaContext();
            var director = new Director 
            {
                Name = Name,
                Country = Country,
            };
            db.Directors.Add(director);
            await db.SaveChangesAsync();

            _directors.Add(director);
        }
    }
}
