//discord highlights bot
//created by CaptainJens
using System.Threading.Tasks;
using System.Drawing;
using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using System.IO;

namespace Koggy
{
    class Program
    {
        static Bot WWWKoggy = new Bot();
        static Bot FriendsKoggy = new Bot();
        static Bot BRKoggy = new Bot();
        static DiscordSocketClient DiscordKoggy = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Verbose });

        static async Task AsyncBot()
        {
            WWWKoggy.SetServer(Bot.ServerType.WWW);
            FriendsKoggy.SetServer(Bot.ServerType.Friends);
            BRKoggy.SetServer(Bot.ServerType.BR);

            await DiscordKoggy.LoginAsync(TokenType.Bot, TOKEN);
            await DiscordKoggy.StartAsync();
            await DiscordKoggy.SetGameAsync("Koggy highlights");
            DiscordKoggy.MessageReceived += MessageReceived;

            await Task.Delay(-1);
        }

        static async Task MessageReceived(SocketMessage Context)
        {
            string Message = Context.Content.ToLower();

            if(Message.StartsWith("koggy highlight"))
            {
                //SEND HIGHLIGHTS HELP MESSAGE AND RETURN
                if(Message == "koggy highlights help" || Message == "koggy highlights" || Message == "koggy highlight help" || Message == "koggy highlight")
                {
                    await Context.Channel.SendMessageAsync("You can create highlights without a video using the command:\n```Koggy highlights <server>, <modelID>, <gameID>, <avatarID>```\n`<server>`: The KoGaMa server: `WWW`, `Friends` or `BR`\n\n`<modelID>`: The ID of the model found in the URL after /marketplace/model/i-\n\n`<gameID>`: The ID of the game found in the URL after /games/play/\n\n`<avatarID>`: The ID of the avatar found in the URL after /marketplace/avatar/a-\n\n__Example__: Koggy highlights WWW, 6817556, 112744, 6956595\n\n\nYou can create highlights that includes a video using the command:\n```Koggy highlights <server>, <modelID>, <gameID>, <avatarID>, <videoID>```\n`<videoID>`: The ID of the YouTube video found in the URL after www.youtube.com/watch?v=\n\n__Example__: Koggy highlights WWW, 6817556, 112744, 6956595, WBIlusTGa5w");
                    return;
                }

                //GET KOGAMA SERVER AND MODEL/AVATAR/GAME ID's  WITH REGEX
                var Matches = Regex.Matches(Message, @"\w+(?=\, )");

                //RETURN IF THERE IS TOO MUCH OR TOO LESS DATA GIVEN
                if (Matches.Count != 3 && Matches.Count != 4)
                {
                    await Context.Channel.SendMessageAsync("I did not recognize the format of the message. The correct format is:```Koggy highlights <server>, <modelID>, <gameID>, <avatarID>, (optitional)<videoID>```To get more information about how to create highlights, use the command: `Koggy highlights help`");
                    return;
                }

                //START TYPING BECAUSE IT MIGHT TAKE A WHILE TO GENERATE THE HIGHLIGHTS
                await Context.Channel.TriggerTypingAsync();

                //VARIABLES
                Bot.Item Model;
                Bot.Game Game;
                Bot.Item Avatar = null;
                Bitmap ModelImage;
                Bitmap GameImage;
                Bitmap AvatarImage;
                Bitmap HighlightsTemplate = null;
                string Video = null;

                //GET MODE/AVATAR/GAME OBJECTS FROM GIVEN ID's
                //(GET VIDEO ID, IF INCLUDED)
                if (Matches[0].Value == "www")
                {
                    Model = await WWWKoggy.GetItem(Bot.ItemType.Model, Matches[1].Value);
                    Game = await WWWKoggy.GetGame(Matches[2].Value);
                    if (Matches.Count == 3)
                    {
                        Avatar = await WWWKoggy.GetItem(Bot.ItemType.Avatar, Message.Substring(Message.LastIndexOf(", ") + 2));
                    }
                    else if(Matches.Count == 4)
                    {
                        Avatar = await WWWKoggy.GetItem(Bot.ItemType.Avatar, Matches[3].Value);
                        Video = Context.Content.Substring(Message.LastIndexOf(", ") + 2);
                    }
                    HighlightsTemplate = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync("https://cdn.discordapp.com/attachments/420915811356508161/422804214226354177/Highlight_of_the_week_Template_BR_low_size.png"));
                }
                else if (Matches[0].Value == "friends")
                {
                    Model = await FriendsKoggy.GetItem(Bot.ItemType.Model, Matches[1].Value);
                    Game = await FriendsKoggy.GetGame(Matches[2].Value);
                    if (Matches.Count == 3)
                    {
                        Avatar = await FriendsKoggy.GetItem(Bot.ItemType.Avatar, Message.Substring(Message.LastIndexOf(", ") + 2));
                    }
                    else if (Matches.Count == 4)
                    {
                        Avatar = await FriendsKoggy.GetItem(Bot.ItemType.Avatar, Matches[3].Value);
                        Video = Context.Content.Substring(Message.LastIndexOf(", ") + 2);
                    }
                    HighlightsTemplate = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync("https://cdn.discordapp.com/attachments/420915811356508161/422804214226354177/Highlight_of_the_week_Template_BR_low_size.png"));
                }
                else if (Matches[0].Value == "br")
                {
                    Model = await BRKoggy.GetItem(Bot.ItemType.Model, Matches[1].Value);
                    Game = await BRKoggy.GetGame(Matches[2].Value);
                    if (Matches.Count == 3)
                    {
                        Avatar = await BRKoggy.GetItem(Bot.ItemType.Avatar, Message.Substring(Message.LastIndexOf(", ") + 2));
                    }
                    else if (Matches.Count == 4)
                    {
                        Avatar = await BRKoggy.GetItem(Bot.ItemType.Avatar, Matches[3].Value);
                        Video = Context.Content.Substring(Message.LastIndexOf(", ") + 2);
                    }
                    HighlightsTemplate = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync("https://cdn.discordapp.com/attachments/420915811356508161/422804212590837761/Highlight_of_the_week_Template_BR.png"));
                }
                else //RETURN IF SERVER IS NOT EQUAL TO WWW, FRIENDS OR BR
                {
                    await Context.Channel.SendMessageAsync("Unknown KoGaMa server. Server must be either `WWW`, `Friends` or `BR`\nTo get more information about how to create highlights, use the command: `Koggy highlights help`");
                    return;
                }

                //RETURN IF MODEL/AVATAR/GAME WAS NOT FOUND
                if(Model == null || Game == null || Avatar == null)
                {
                    await Context.Channel.SendMessageAsync("Something went wrong. Are you sure that you sent the right model/game/avatar/video ID?\nTo get more information about how to create highlights, use the command: `Koggy highlights help`");
                    return;
                }

                //GET MODEL/AVATAR/GAME IMAGES
                ModelImage = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync(Model.Images.Large));
                GameImage = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync(Game.Images.Medium));
                AvatarImage = (Bitmap)Bitmap.FromStream(await WWWKoggy.Client.GetStreamAsync(Avatar.Images.Large));

                //BUILD HIGHLIGHTS IMAGE
                Bitmap HighlightsImage = new Bitmap(HighlightsTemplate.Width, HighlightsTemplate.Height);
                using (Graphics HighlightsBuilder = Graphics.FromImage(HighlightsImage))
                {
                    HighlightsBuilder.DrawImage(ModelImage, 86, 330, 1011, 1011);
                    HighlightsBuilder.DrawImage(GameImage, 1173, 365, 973, 977);
                    HighlightsBuilder.DrawImage(AvatarImage, 2225, 331, 1011, 1011);
                    HighlightsBuilder.DrawImage(HighlightsTemplate, 0, 0, HighlightsTemplate.Width, HighlightsTemplate.Height);
                }
                HighlightsImage.Save("highlights.png");

                //SEND HIGHLIGHTS IMAGE
                await Context.Channel.SendFileAsync("highlights.png", Matches[0].Value.ToUpper() + " HIGHLIGHTS" + ((Video == null) ? " (without video)" : ""));

                //HIGHLIGHTS TEXT
                //WWW
                if (Matches[0].Value == "www")
                {
                    string Members = null;
                    foreach (Bot.User Member in Game.Members)
                    {
                        Members += " - [" + Member.Name +"](http://www.kogama.com/profile/" + Member.ID + "/)\n";
                    }

                    //ENGLISH
                    string HighlightsEnglish = "\nHello Kogamians!\n\nWhat do you think of these highlights? Congratulation to the winners!\n\n\n### Game of the Week\n\n";
                    HighlightsEnglish += "[![](" + Game.Images.Large + ")](http://www.kogama.com/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com/games/play/" + Game.ID + "/)\n\n";
                    HighlightsEnglish += "**Members:**\n\n" + Members + "\n";

                    HighlightsEnglish += "### Model of the week\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsEnglish += "**Created by:** [" + Model.Author.Name + "](http://www.kogama.com/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsEnglish += "### Avatar of the week\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsEnglish += "**Created by:** [" + Avatar.Author.Name + "](http://www.kogama.com/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsEnglish += "### Video\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsEnglish += "### KoGaMa Community News\n\n";
                    HighlightsEnglish += "We are looking for awesome highlights from the community. The models and avatars doesn't have to be detailed, and the games doesn’t have to be large. We are also looking for creations that show creativity and offer fun experiences. Everyone can participate! We are highlighting our favorites! We are looking forward to see your creations :-)\n\n";
                    HighlightsEnglish += "Email YouTube videos, model, avatar or game suggestions to [cm@kogama.com](mailto:cm@kogama.com)\n\n**Winners will receive 50.000 exp and a cool badge to show off!**\n\nKeep rocking!\n\n**[Follow us on Twitter!](https://twitter.com/kogamagame)**\n**[Like us on Facebook!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_english.txt", HighlightsEnglish);
                    await Context.Channel.SendFileAsync("highlights_english.txt");


                    //POLISH
                    string HighlightsPolish = "\nWitajcie KoGaMianie!\n\nCo myślicie o tym podsumowaniu? Gratulujemy zwycięzcom!\n\n\n### Gra Tygodnia\n\n";
                    HighlightsPolish += "[![](" + Game.Images.Large + ")](http://www.kogama.com/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com/games/play/" + Game.ID + "/)\n\n";
                    HighlightsPolish += "**Członkowie:**\n\n" + Members + "\n";

                    HighlightsPolish += "### Model Tygodnia\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsPolish += "**Twórca:** [" + Model.Author.Name + "](http://www.kogama.com/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsPolish += "### Awatar Tygodnia\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsPolish += "**Twórca:** [" + Avatar.Author.Name + "](http://www.kogama.com/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsPolish += "### Filmik\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsPolish += "### Podsumowania Tygodnia w KoGaMa\n\n";
                    HighlightsPolish += "Szukamy jak najlepszych dzieł, stworzonych przez społeczność. Modele i awatary nie muszą odznaczać się zbytnią liczbą detali i gry nie muszą być duże. Poszukujemy także tych, które pokazują kreatywność i oferują ciekawe doświadczenia z nimi. Każdy może wziąć udział! Wyróżniamy te, które najbardziej nam się spodobają! Jesteśmy ciekawi, co stworzyliście w świecie KoGaMa :-)\n\n";
                    HighlightsPolish += "Wysyłaj linki do swoich filmików z YouTube, modeli, awatarów i gier do [cm@kogama.com](mailto:cm@kogama.com)\n\n**Zwycięzcy otrzymają 50.000 PD i fajną odznakę, którą będą mogli się pochwalić!**\n\nDajcie czadu!\n\n**[Śledźcie nasz Twitter!](https://twitter.com/kogamagame)**\n**[Polubcie nas na Facebooku!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_polish.txt", HighlightsPolish);
                    await Context.Channel.SendFileAsync("highlights_polish.txt");


                    //GERMAN
                    string HighlightsGerman = "\nHallo Kogamianer!\n\nWas hältst du von diesen Highlights? Herzlichen Glückwunsch an die Gewinner!\n\n\n### Spiel der Woche\n\n";
                    HighlightsGerman += "[![](" + Game.Images.Large + ")](http://www.kogama.com/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com/games/play/" + Game.ID + "/)\n\n";
                    HighlightsGerman += "**Mitglieder:**\n\n" + Members + "\n";

                    HighlightsGerman += "### Modell der Woche\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsGerman += "**Erstellt von:** [" + Model.Author.Name + "](http://www.kogama.com/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsGerman += "### Avatar der Woche\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsGerman += "**Erstellt von:** [" + Avatar.Author.Name + "](http://www.kogama.com/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsGerman += "### Video\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsGerman += "### KoGaMa Community News\n\n";
                    HighlightsGerman += "Wir suchen nach tollen Highlights von der Community. Die Modelle und Avatare müssen nicht detailliert sein, und die Spiele müssen nicht groß sein. Wir suchen außerdem nach Kreationen, die Kreativität zeigen und spaßige Erfahrungen anbieten. Jeder kann mitmachen! Wir zeigen unsere Favoriten in den Highlights! Wir freuen uns darauf, deine Kreationen zu sehen. :-)\n\n";
                    HighlightsGerman += "Sende uns YouTube Videos, Modell-, Avatar- oder Spielvorschläge per E-Mail an [cm@kogama.com](mailto:cm@kogama.com)\n\n**Die Gewinner erhalten 50.000 Exp und erhalten coole Abzeichen zum angeben!!**\n\nRockt weiter!\n\n**[Folge uns auf Twitter!](https://twitter.com/kogamagame)**\n**[Like uns auf Facebook!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_german.txt", HighlightsGerman);
                    await Context.Channel.SendFileAsync("highlights_german.txt");


                    //FRENCH
                    string HighlightsFrench = "\nSalut KoGaMiens!\n\nQue pensez-vous des chef-d’oeuvres de cette semaine? Félicitations aux gagnants!\n\n\n### Jeu de la Semaine\n\n";
                    HighlightsFrench += "[![](" + Game.Images.Large + ")](http://www.kogama.com/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com/games/play/" + Game.ID + "/)\n\n";
                    HighlightsFrench += "**Membres:**\n\n" + Members + "\n";

                    HighlightsFrench += "### Modèle de la Semaine\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsFrench += "**Crée par:** [" + Model.Author.Name + "](http://www.kogama.com/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsFrench += "### Avatar de la Semaine\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsFrench += "**Crée par:** [" + Avatar.Author.Name + "](http://www.kogama.com/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsFrench += "### Vidéo\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsFrench += "### Nouvelles de la Communauté de KoGaMa\n\n";
                    HighlightsFrench += "Nous recherchons de nouveaux chef-d’oeuvres impressionnants dans la communauté KoGaMa. Les modèles et les avatars n'ont pas besoin d'être détaillés, et les jeux n'ont pas besoin d'être grands. Nous recherchons également des créations qui font preuve de créativité et offrant une expérience amusante. Tout le monde peut participer! Nous mettons en évidence nos favoris! Nous sommes impatients de voir vos créations :-)\n\n";
                    HighlightsFrench += "Envoyez par E-mail des suggestions de vidéos YouTube, de modèles, d'avatars ou de jeux à [cm@kogama.com](mailto:cm@kogama.com)\n\n**Les gagnants recevront 50.000 points d'expérience et un badge cool à montrer!**\n\nContinuez de gérer!\n\n**[Suivez-nous sur Twitter!](https://twitter.com/kogamagame)**\n**[Likez-nous sur Facebook!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_french.txt", HighlightsFrench);
                    await Context.Channel.SendFileAsync("highlights_french.txt");

                    //Portugese
                    string HighlightsPortugese = "\nE aí Kogamians!\n\nO que vocês acham desses destaques? Parabéns aos vencedores!\n\n\n### Jogo da semana\n\n";
                    HighlightsPortugese += "[![](" + Game.Images.Large + ")](http://www.kogama.com/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com/games/play/" + Game.ID + "/)\n\n";
                    HighlightsPortugese += "**Membros:**\n\n" + Members + "\n";

                    HighlightsPortugese += "### Item da semana\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsPortugese += "**Criado por:** [" + Model.Author.Name + "](http://www.kogama.com/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsPortugese += "### Avatar da semana\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsPortugese += "**Criado por:** [" + Avatar.Author.Name + "](http://www.kogama.com/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsPortugese += "### Vídeo\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsPortugese += "### Notícias da comunidade\n\n";
                    HighlightsPortugese += "Estamos à procura de incríveis criações da nossa comunidade. Os itens e avatares não precisam ser detalhados, e os jogos não precisam ser grandes. We are also looking for creations that show creativity and offer fun experiences. Todos podem participar! Estamos destacando nossos favoritos! Aguardaremos ansiosos para ver suas criações :-)\n\n";
                    HighlightsPortugese += "Envie sugestões de vídeos do YouTube, itens, avatares ou jogos para [cm@kogama.com](mailto:cm@kogama.com)\n\n**Os vencedores receberão 50.000 xp e um emblema maneiro!**\n\nFiquem bem!\n\n**[Siga-nos no Twitter!](https://twitter.com/kogamagame)**\n**[Curta-nos no Facebook!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_portugese.txt", HighlightsPortugese);
                    await Context.Channel.SendFileAsync("highlights_portugese.txt");

                }

                //BR
                else if(Matches[0].Value == "br")
                {
                    string Members = null;
                    foreach (Bot.User Member in Game.Members)
                    {
                        Members += " - [" + Member.Name + "](http://www.kogama.com.br/profile/" + Member.ID + "/)\n";
                    }

                    //Portugese
                    string HighlightsPortugese = "\nE aí Kogamians!\n\nO que vocês acham desses destaques? Parabéns aos vencedores!\n\n\n### Jogo da semana\n\n";
                    HighlightsPortugese += "[![](" + Game.Images.Large + ")](http://www.kogama.com.br/games/play/" + Game.ID + "/)\n[" + Game.Name + "](http://www.kogama.com.br/games/play/" + Game.ID + "/)\n\n";
                    HighlightsPortugese += "**Membros:**\n\n" + Members + "\n";

                    HighlightsPortugese += "### Item da semana\n\n[![](" + Model.Images.Large + ")](http://www.kogama.com.br/marketplace/model/i-" + Model.ID + "/)\n[" + Model.Name + "](http://www.kogama.com.br/marketplace/model/i-" + Model.ID + "/)\n\n";
                    HighlightsPortugese += "**Criado por:** [" + Model.Author.Name + "](http://www.kogama.com.br/profile/" + Model.Author.ID + "/)\n\n";

                    HighlightsPortugese += "### Avatar da semana\n\n[![](" + Avatar.Images.Large + ")](http://www.kogama.com.br/marketplace/avatar/a-" + Avatar.ID + "/)\n[" + Avatar.Name + "](http://www.kogama.com.br/marketplace/avatar/a-" + Avatar.ID + "/)\n\n";
                    HighlightsPortugese += "**Criado por:** [" + Avatar.Author.Name + "](http://www.kogama.com.br/profile/" + Avatar.Author.ID + "/)\n\n";

                    if (Video != null)
                    {
                        HighlightsPortugese += "### Vídeo\n\n<iframe src=\"https://www.youtube.com/embed/" + Video + "\" width=\"640\" height=\"360\" frameborder=\"0\" allowfullscreen></iframe>\n\n";
                    }
                    HighlightsPortugese += "### Notícias da comunidade\n\n";
                    HighlightsPortugese += "Estamos à procura de incríveis criações da nossa comunidade. Os itens e avatares não precisam ser detalhados, e os jogos não precisam ser grandes. We are also looking for creations that show creativity and offer fun experiences. Todos podem participar! Estamos destacando nossos favoritos! Aguardaremos ansiosos para ver suas criações :-)\n\n";
                    HighlightsPortugese += "Envie sugestões de vídeos do YouTube, itens, avatares ou jogos para [cm@kogama.com](mailto:cm@kogama.com)\n\n**Os vencedores receberão 50.000 xp e um emblema maneiro!**\n\nFiquem bem!\n\n**[Siga-nos no Twitter!](https://twitter.com/kogamagame)**\n**[Curta-nos no Facebook!](https://www.facebook.com/Kogamians/?fref=news)**";

                    File.WriteAllText("highlights_portugese.txt", HighlightsPortugese);
                    await Context.Channel.SendFileAsync("highlights_portugese.txt");
                }
            }
        }

        static void Main(string[] args)
        {
            AsyncBot().Wait();
        }
    }
}
