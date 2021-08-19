module Domain =
    type Direction = North | East | South | West
    type MoveCommand = Left | Right | Forward

    type ActiveJellyFish = 
        { Position: int * int
          Facing: Direction }

    type LostJellyFish = 
        { Scent: int * int
          Facing: Direction }

    type JellyFish = 
        | Active of ActiveJellyFish
        | Lost of LostJellyFish

    type Game =
        { TankBoundary: int * int
          JellyFish: JellyFish }
    
    type GameAndMoves =
        { Game: Game
          Commands: MoveCommand list }

    // Active pattern for checking if a position is 'outside' of the jellyfish tank
    let (|OutsideOfTank|_|) (boundary: int * int) position =
        let xmax, ymax = boundary
        match position with
        | x, y when 
            x > xmax 
            || y > ymax
            || x < 0
            || y < 0 -> Some OutsideOfTank
        | _ -> None

module UseCase =
    open Domain

    let rotateRight (jellyFish: ActiveJellyFish) =
        match jellyFish.Facing with
        | North -> { jellyFish with Facing = East }
        | East ->  { jellyFish with Facing = South }
        | South -> { jellyFish with Facing = West }
        | West ->  { jellyFish with Facing = North }

    let rotateLeft (jellyFish: ActiveJellyFish) =
        match jellyFish.Facing with
        | North -> { jellyFish with Facing = West }
        | East ->  { jellyFish with Facing = North }
        | South -> { jellyFish with Facing = East }
        | West ->  { jellyFish with Facing = South }
    
    let getNextPosition (jellyFish: ActiveJellyFish) =
        let x, y = jellyFish.Position
        match jellyFish.Facing with
        | North -> x, y + 1
        | South -> x, y - 1
        | East ->  x + 1, y
        | West ->  x - 1, y

    let moveForward boundary (jellyFish: ActiveJellyFish) =
        let nextPosition = getNextPosition jellyFish
        match nextPosition with
        | OutsideOfTank boundary ->
            Lost { Scent = jellyFish.Position; Facing = jellyFish.Facing }
        | _ -> Active { jellyFish with Position = nextPosition }

    let moveFish command (game: Game) (fish: ActiveJellyFish) =
        match command with
        | Left ->    { game with JellyFish = Active (rotateLeft fish) }
        | Right ->   { game with JellyFish = Active (rotateRight fish) }
        | Forward -> { game with JellyFish = moveForward game.TankBoundary fish }

    let updateGame command game =
        match game.JellyFish with
        | Active fish -> moveFish command game fish
        | Lost _ -> game

module Input =
    open Domain
    open System.IO

    let explode (s: string) =
        [ for c in s -> c.ToString() ]
       
    let readTankDimensions (line: string) =
        let coords = line |> explode |> List.map int
        coords.[0], coords.[1]
    
    let parseFacingDirection (input:string) =
        match input with
        | "N" -> North
        | "S" -> South
        | "E" -> East
        | "W" -> West
        | _ -> invalidArg "JellyFish Direction" "Jellyfish much face: N, S, E, or W" 
    
    let readJellyFishStart (line: string) =
        let initialPosition = line |> explode
        match initialPosition with
        | [ x; y; facing ] ->
            { Position = int x, int y; Facing = parseFacingDirection facing }
        | _ -> invalidArg "Initial JellyFish Location" "Must be coordinates and a direction ie: 11E"
    
    let parseJellyfishCommand (input: string) =
        match input with
        | "R" -> Right
        | "L" -> Left
        | "F" -> Forward
        | _ -> invalidArg "Jellyfish move ment commands" "Must be one of R, L, or F"
    
    let readJellyFishCommands (line: string) =
        line |> explode |> List.map parseJellyfishCommand
    
    let readFile () =
        let path = $"{Directory.GetCurrentDirectory()}\jellyfish.txt"
        seq {
            use sr = new StreamReader(path)
            while not sr.EndOfStream do
                yield sr.ReadLine()
        } |> Seq.toList
    
    let createGame (input: string list) =
        let dimensions = Seq.head input |> readTankDimensions
        List.chunkBySize 2 input.Tail
        |> List.map (fun l -> l.[0], l.[1] )
        |> List.map (fun (a, b) -> readJellyFishStart a, readJellyFishCommands b)
        |> List.map (fun (start, commands) ->
            { Game = { TankBoundary = dimensions; JellyFish = Active start }
              Commands = commands })

open Domain
open UseCase
open Input
open System

let printJellyfish (jellyfish: JellyFish) =
    match jellyfish with
    | Active fish ->
        let x, y = fish.Position
        printfn $"{x}{y}{fish.Facing}"
    | Lost fish ->
        let x, y = fish.Scent
        printfn $"{x}{y}{fish.Facing}LOST"

let playGame () =
    let input = readFile()
    let games = createGame input
    games |> List.iter (fun gameAndMoves ->
        let mutable gameState = gameAndMoves.Game
        for command in gameAndMoves.Commands do
            gameState <- updateGame command gameState
        printJellyfish gameState.JellyFish
        ())

Console.WriteLine("=== Jellyfish ===")
playGame()
