using NoRussian;

const string figlet = @"
 _   _       ____                _
| \ | | ___ |  _ \ _   _ ___ ___(_) __ _ _ __
|  \| |/ _ \| |_) | | | / __/ __| |/ _` | '_ \
| |\  | (_) |  _ <| |_| \__ \__ | | (_| | | | |
|_| \_|\___/|_| \_\\__,_|___|___|_|\__,_|_| |_|

";

Console.WriteLine(figlet);

var links = File.ReadAllLines(Directory.GetCurrentDirectory() + "/NoRussian/data.txt");

var worker = new Worker(links.Length);

foreach (var link in links) worker.Post(link);

worker.Wait();