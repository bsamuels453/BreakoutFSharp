namespace global

module Control =
    open SFML.Window;
    open System;
    let getKeyState keyboardState desiredKey =
        let matchKey key keyComp =
            match keyComp with
            | {KeyStateChange.Key = skey} when skey=key -> true
            | _ -> false

        keyboardState
        |> Array.find (matchKey desiredKey)



    let pollKeyboard() =
        let arrayBuilder = new ArrayBuilder()
        let keys = Enum.GetNames(typedefof<Keyboard.Key>)
        let keyVals = Array.map (fun k -> Enum.Parse(typedefof<Keyboard.Key>, k) :?> Keyboard.Key ) keys
    
        arrayBuilder{
            for key in keyVals do
                match Keyboard.IsKeyPressed(key) with
                | true -> yield {KeyState=Pressed; Key=key}
                | _ -> yield {KeyState=Released; Key=key}
        }