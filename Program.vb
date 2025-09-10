Imports System.IO
Imports System.Reflection

Module PokemonRandomizer

    ' Given that future forms may be added I decided that the way they were generated previously (pulled from a txt file) was too rigid
    ' I'm not happy with this solution, but it'll work for now. Until a better solution is found expand as needed.
    Dim PokemonForms As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase) From {
        {"Rotom", New List(Of String) From {"Naked", "Heat", "Wash", "Frost", "Fan", "Mow"}},
        {"Silvally", New List(Of String) From {
            "Normal", "Fire", "Water", "Electric", "Grass", "Ice", "Fighting",
            "Poison", "Ground", "Flying", "Psychic", "Bug", "Rock", "Ghost",
            "Dragon", "Dark", "Steel", "Fairy"
        }},
        {"Slowbro", New List(Of String) From {"Kanto", "Galar"}},
        {"Slowking", New List(Of String) From {"Kanto", "Galar"}}
    }

    Sub Main()
        Console.WriteLine("Open35 version 1.35: a Friendly Random Meta Generator")
        Console.WriteLine()

        ' I decided to make 35 Pokes more user friendly by having it create a list text file on launch if one doesn't exist
        ' This way if no list exists it'll tell the user to make one rather than crashing without explanation
        Dim programFolder As String = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)
        Dim fileName As String = "Rollable Pokes List.txt"
        Dim filePath As String = Path.Combine(programFolder, fileName)

        If Not File.Exists(filePath) Then
            Try
                Dim header As String = String.Join(Environment.NewLine, New String() {
                    "No list found. Creating empty list file.",
                    "---------------------",
                    "Find 'Rollable Pokes List.txt' and enter one Pokémon name per line in this file. Like This:",
                    "",
                    "Yanmega",
                    "Galvantula",
                    "Quagsire",
                    "Clodsire",
                    "Rotom",
                    "",
                    "The official 35 Pokes rollable list can be found in the 35-Info channel of the official Discord.",
                    "After filling this file to your satisfaction, start this program again and Open35 should work as intended.",
                    ""
                })
                File.WriteAllText(filePath, header)

                Console.ForegroundColor = ConsoleColor.Yellow
                Console.WriteLine("No ""{0}"" was found in the program folder.", fileName)
                Console.ResetColor()
                Console.WriteLine("")
                Console.WriteLine("A file named ""{0}"" has been created in:", fileName)
                Console.WriteLine(filePath)
                Console.WriteLine("")
                Console.WriteLine("Please open that file, add one Pokémon name per line, save it, then re-run this program.")
                Console.WriteLine("")
                Console.WriteLine("Press Enter to exit...")
                Console.ReadLine()
            Catch ex As Exception
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine("Failed to create the file: " & ex.Message)
                Console.ResetColor()
                Console.WriteLine("Press Enter to exit...")
                Console.ReadLine()
            End Try

            Return
        End If

        Dim rawLines As String() = File.ReadAllLines(filePath)
        Dim allPokemonList As New List(Of String)

        For Each line As String In rawLines
            Dim trimmed As String = If(line, "").Trim()
            If trimmed <> "" Then
                allPokemonList.Add(trimmed)
            End If
        Next

        If allPokemonList.Count = 0 Then
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("The file ""{0}"" exists but contains no Pokémon.", fileName)
            Console.ResetColor()
            Console.WriteLine("Please open the file, add one Pokémon name per line, save it, then re-run this program.")
            Console.WriteLine("")
            Console.WriteLine("Press Enter to exit...")
            Console.ReadLine()
            Return
        End If

        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim availablePokemon As New List(Of String)
        For Each poke As String In allPokemonList
            If Not seen.Contains(poke) Then
                seen.Add(poke)
                availablePokemon.Add(poke)
            End If
        Next

        Dim numToSelect As Integer = 0
        While True
            Console.Write("How many Pokémon do you want to roll?")
            Dim input As String = Console.ReadLine().Trim()
            If Integer.TryParse(input, numToSelect) Then
                If numToSelect > 0 Then
                    Exit While
                End If
            End If
            Console.WriteLine("Please enter a positive integer.")
        End While

        ' Banlist was pretty easy to add actually, here's my attempt at it. If this is horrible spaghetti feel free to change tho
        Console.Write("Enter banned Pokémon (comma separated, leave blank for none): ")
        Dim banInput As String = Console.ReadLine()
        Dim bannedSet As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        If Not String.IsNullOrWhiteSpace(banInput) Then
            Dim parts = banInput.Split(","c)
            For Each p In parts
                Dim t = p.Trim()
                If t <> "" Then
                    bannedSet.Add(t)
                End If
            Next
        End If

        Dim filtered As New List(Of String)
        For Each p In availablePokemon
            If Not bannedSet.Contains(p) Then
                filtered.Add(p)
            End If
        Next

        ' Just in case the unthinkable happens and some one asks for one 35 Million Pokes
        If filtered.Count < numToSelect Then
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("")
            Console.WriteLine("There aren't enough valid Pokemon to meet your demand.")
            Console.ResetColor()
            Console.WriteLine("Available: {0}. Requested: {1}.", filtered.Count, numToSelect)
            Console.WriteLine("Either update the file or adjust the ban list and try again.")
            Console.WriteLine("")
            Console.WriteLine("Press enter to exit")
            Console.ReadLine()
            Return
        End If

        Dim rng As New Random()
        ShuffleList(filtered, rng)

        Dim chosenBase As List(Of String) = filtered.Take(numToSelect).ToList()

        Dim output As New List(Of String)
        For Each baseName In chosenBase
            If PokemonForms.ContainsKey(baseName) Then
                Dim forms = PokemonForms(baseName)
                If forms IsNot Nothing AndAlso forms.Count > 0 Then
                    Dim idx As Integer = rng.Next(forms.Count)
                    output.Add(String.Format("{0}-{1}", baseName, forms(idx)))
                Else
                    output.Add(baseName)
                End If
            Else
                output.Add(baseName)
            End If
        Next

        Console.WriteLine("")
        Console.WriteLine("=== Pokes ===")
        For Each line As String In output
            Console.WriteLine(line)
        Next

        Console.WriteLine("")
        Console.WriteLine("Press Enter to exit")
        Console.ReadLine()
    End Sub

    Private Sub ShuffleList(Of T)(list As List(Of T), rng As Random)
        Dim n As Integer = list.Count
        While n > 1
            n -= 1
            Dim k As Integer = rng.Next(n + 1)
            Dim tmp As T = list(k)
            list(k) = list(n)
            list(n) = tmp
        End While
    End Sub

End Module