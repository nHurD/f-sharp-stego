(* 
	stego.fsx - A simple F# implementation of a steganographic library.
	Author: Jonathan R. Steele <jrsteele@gmail.com>
*)
open System
open System.IO
open System.Text

open System.Drawing


let getBit (bit:byte) (pos:int) :byte = 
    let truePos = 8 - pos
    ((bit &&& ( byte 1 <<< truePos)) >>> truePos)

let replaceBit (bit:byte) (pos:int) (value:byte) :byte =
   let tmp = int value
   match tmp with
     | 1 -> bit ||| ( byte 1 <<< pos )
     | _ -> bit &&& ~~~( byte 1 <<< pos )
     
let rec setData (message:byte[]) (img:Bitmap) (msgIdx:int) (blockCtr:int) (imgCntr:int) =
   match msgIdx with
     | msgIdx when msgIdx < message.Length ->
       let imgX = imgCntr % img.Width
       let imgY = imgCntr / img.Width
       let pix = img.GetPixel( imgX, imgY )
       let msgBit = match imgX with
                       | imgX when (imgX = 0 && imgY = 0) -> byte message.Length
                       | _ -> getBit message.[msgIdx] blockCtr 
       let newBit = match imgX with
                      | imgX when (imgX = 0 && imgY = 0) -> msgBit
                      | _ -> replaceBit pix.R 0 msgBit
       let newPix = Color.FromArgb( int pix.A, int newBit, int pix.G, int pix.B )
       img.SetPixel( imgX, imgY, newPix )
       let newIdx = if blockCtr = 8 then msgIdx + 1 else msgIdx
       let newBCntr = match imgX with
                        | imgX when (imgX = 0 && imgY = 0) -> blockCtr
                        | _ -> if blockCtr = 8 then 0 else (blockCtr + 1)
       let newCntr = imgCntr + 1
       setData message img newIdx newBCntr newCntr
     | _ -> ()

/// Build a byte and return it
let rec getData (dest:byte) (img:Bitmap) (blockCtr:int) (imgCntr:int) : byte =
    match blockCtr with
      | blockCtr when blockCtr <= 8 -> 
          let imgX = imgCntr % img.Width
          let imgY = imgCntr / img.Width
          let pix = img.GetPixel( imgX, imgY )
          let newDest = ( dest <<< 1 ) ||| (getBit pix.R 8)
          let newBlock = blockCtr + 1
          let newIctr = imgCntr + 1
          getData newDest img newBlock newIctr
      | _ -> dest

/// Fetch a message from a given image
let decodeImage (inFile:string) =
    let img = Image.FromFile(inFile)
    let drawing = new Bitmap(img)
    
    let zeroPix = drawing.GetPixel(0,0)
    
    let msgSize = int zeroPix.R
    
    let msg = [| for i in 1 .. 9 .. (msgSize*9) -> getData (byte 0) drawing 0 i |]
    
    let res = System.Text.Encoding.ASCII.GetString(msg)
    
    printfn "'%s'" res
    
    drawing.Dispose()
    img.Dispose()
  
/// Process a given image
let encodeImage (inFile:string) (outFile:string) (outFormat:System.Drawing.Imaging.ImageFormat) (msg:string) =
    let img = Image.FromFile(inFile)
    let drawing = new System.Drawing.Bitmap (img)
    let msgData = Encoding.ASCII.GetBytes(msg)

    setData msgData drawing 0 0 0
    
    drawing.Save (outFile, outFormat)
       
    drawing.Dispose()
    img.Dispose()

encodeImage "riding.jpg" "test.png" System.Drawing.Imaging.ImageFormat.Png "This is a test!"
//decodeImage "test.jpg"

