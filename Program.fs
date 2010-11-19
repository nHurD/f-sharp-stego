
module Main
   open System
   open System.Drawing.Imaging
   
   [<EntryPoint>]
   let main args =
       let boilerPlate = 
            "Usage: f-stego-sharp.exe <operation> [inFile] [outFile] [outFormat] [message]\n\t operation: encode -> Encode [message] into [inFile] and save it to [outFile] int [outFormat] \n\t\t    decode -> Decode an image from [inFile]\n" +
            "\t Supported Formats: PNG, BMP, TIFF"
       
       match args.Length with
           | 0 -> printfn "%s" boilerPlate
                  1
           | _ -> match args.[0].ToUpper() with
                    | "ENCODE" -> match args.Length with
                                     | x when x < 5 -> 
                                             printfn "%s" boilerPlate
                                             1
                                     | _ -> 
                                            let imgFormat = match args.[3].ToUpper() with
                                                               | "BMP" -> ImageFormat.Bmp
                                                               | "TIFF" -> ImageFormat.Tiff
                                                               | "PNG" -> ImageFormat.Png
                                                               | _ -> raise (System.ArgumentException("Unsupported Format Given"))
                                            fSharpStego.encodeImage args.[1] args.[2] imgFormat args.[4]
                                            0
                    | "DECODE" -> match args.Length with
                                    | x when x < 2 ->
                                           printfn "%s" boilerPlate
                                           1
                                    | _ -> 
                                           let res = fSharpStego.decodeImage args.[1]
                                           printfn "%s" res
                                           0
                    | _ -> printfn "%s" boilerPlate
                           1
       



