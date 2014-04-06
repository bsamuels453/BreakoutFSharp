namespace global

module Resource =
    open SFML.Graphics;
    open SFML.Audio;

    let loadTextures() =
        let listbuilder = new ListBuilder()
        listbuilder{
            yield "red", new Texture("redsplotch.jpg")
            yield "orange", new Texture("orangesplotch.jpg")
            yield "yellow", new Texture("yellowsplotch.jpg")
            yield "green", new Texture("greensplotch.jpg")
            yield "cyan", new Texture("cyansplotch.jpg")
            yield "blue", new Texture("bluesplotch.jpg")
            yield "purple", new Texture("purplesplotch.jpg")
        }

    let loadSounds() =
        let listbuilder = new ListBuilder()
        listbuilder{
            yield "hit", new SoundBuffer("hit.wav")
            yield "blip", new SoundBuffer("blip.wav")
            yield "squareblip", new SoundBuffer("squareblip.wav")
        }

    let extractResource resourceList desiredResourceName =
        let matchText text comp =
            match comp with
            | (t,_) when t = text -> true
            | _ -> false
        let (_,texture) =
            resourceList
            |> List.find (matchText desiredResourceName)
        texture