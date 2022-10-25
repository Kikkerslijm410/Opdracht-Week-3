using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncBoekOpdracht
{
    class Boek
    {
        public string? Titel { get; set; }
        public string? Auteur { get; set; }

        /*
        wordt niet meer gebruikt omdat we nu async werken
        public float AIScore {
            get {
                double ret = 1.0f;
                for (int i = 0; i < 10000000; i++)
                    for (int j = 0; j < 10; j++)
                        ret = (ret + Willekeurig.Random.NextDouble()) % 1.0;
                return (float)ret;
            }
         }
        */

        /* 
        async versie van AIScore
        Tuple is een struct die 2 waarden kan bevatten,
        dus kan hij in dit geval de score en het boek bevatten.
        Wat ervoor zorgt dat we niet 2 lijsten hoeven te maken
        en dat we de score en het boek kunnen opslaan in 1 lijst
        en dat we dus niet hoeven te zoeken naar de bijbehorende score
        bij het boek.
        */
        public async Task<Tuple<float,Boek>> AIScore(){
            Task<Tuple<float,Boek>> calculate = Task.Run(() =>{
                double ret = 1.0f;
                for (int i = 0; i < 10000000; i++)
                    for (int j = 0; j < 10; j++)
                        ret = (ret + Willekeurig.Random.NextDouble()) % 1.0;
                return Tuple.Create((float)ret, this);
            });
            Tuple<float,Boek> result = await calculate;
            return result;     
        }
    }

    static class Willekeurig
    {
        public static Random Random = new Random();
        public static async Task Vertraging(int MinAantalMs = 500, int MaxAantalMs = 1000)
        {
            await Task.Delay(Random.Next(MinAantalMs, MaxAantalMs));
        }
    }
    static class Database
    {
        private static List<Boek> lijst = new List<Boek>();
        public static async void VoegToe(Boek b)
        {
            await Willekeurig.Vertraging(); // INSERT INTO ...
            lijst.Add(b);
        }
        public static async Task<List<Boek>> HaalLijstOp()
        {
            await Willekeurig.Vertraging(); // SELECT * FROM ...
            return lijst;
        }
        public static async void Logboek(string melding)
        {
            await Willekeurig.Vertraging(); // schrijf naar logbestand
        }
    }
    class Program
    {
        static async Task VoegBoekToe() {
            int i = 1;
            Console.WriteLine("Geef de titel op: ");
            var titel = Console.ReadLine();
            Console.WriteLine("Geef de auteur op: ");
            var auteur = Console.ReadLine();
            Console.Clear();
            Database.VoegToe(new Boek {Titel = titel, Auteur = auteur});
            Database.Logboek("Er is een nieuw boek!");
            Console.WriteLine("De huidige lijst met boeken is: ");
            foreach (var boek in await Database.HaalLijstOp()) {
                Console.WriteLine(i + ") " + boek.Titel);
                i++;
            }
        }
        static async Task ZoekBoek() {
            Console.WriteLine("Waar gaat het boek over?");
            var beschrijving = Console.ReadLine();
            Boek? beste = null;
            float AIScore = new float();

            var TaskManager = new List<Task<Tuple<float,Boek>>>();

            foreach (var boek in (await Database.HaalLijstOp()).ToList()){
                TaskManager.Add(boek.AIScore());
            }
            foreach(var task in await(Task.WhenAll(TaskManager))){
                if (task.Item1 > AIScore)
                    AIScore = task.Item1;
                    beste = task.Item2;
            }
            if(beste != null){
                Console.WriteLine("De beste overeenkomst is: ");
                Console.WriteLine(beste.Titel);
                await Task.Delay(10000);
            }
            else{
                Console.WriteLine("Geen overeenkomst gevonden.");
                await Task.Delay(3000);
            }      
        }
        static bool Backupping = false;
        static async Task Backup() {
            if (Backupping)
                return;
            Backupping = true;
            await Willekeurig.Vertraging(2000, 3000);
            Backupping = false;
            Console.WriteLine("//Backup Gemaakt");
        }
        static async Task Main(string[] args)
        {
            Database.VoegToe(new Boek {Titel = "De Hobbit", Auteur = "J.R.R. Tolkien"});
            Database.VoegToe(new Boek {Titel = "De Grote Gatsby", Auteur = "F. Scott Fitzgerald"});
            Database.VoegToe(new Boek {Titel = "De Kleine Prins", Auteur = "Antoine de Saint-Exupéry"});
            Database.VoegToe(new Boek {Titel = "De Schaduw van de Wind", Auteur = "Carlos Ruiz Zafón"});
            Database.VoegToe(new Boek {Titel = "De Gelaarsde Kat", Auteur = "Antoine de Saint-Exupéry"});

            string? key = null;
            while (key != "D" && key != "d") {
                Console.WriteLine("Welkom bij de boeken administratie!");
                Console.WriteLine("A) Boek toevoegen");
                Console.WriteLine("B) Boek zoeken");
                Console.WriteLine("C) Backup maken van de boeken");
                Console.WriteLine("D) Quit");
                key = Console.ReadLine();
                Console.Clear();
                if (key == "A" || key == "a")
                    await VoegBoekToe();
                else if (key == "B" || key == "b")
                    await ZoekBoek();
                else if (key == "C" || key == "c")
                    Backup();
                else Console.WriteLine("Ongeldige invoer!");
            }
        }
    }
}