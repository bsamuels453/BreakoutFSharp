namespace global

module Resource =
    open SFML.Graphics;

    let LoadTextures() =
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

    let GetTexture textures desiredTexture =
        let matchText text comp =
            match comp with
            | (t,_) when t = text -> true
            | _ -> false
        let (_,texture) =
            textures
            |> List.find (matchText desiredTexture)
        texture