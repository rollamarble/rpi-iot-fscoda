[<CoDa.Code>]
module Fsc.App
 
open System
open System.IO
open CoDa.Runtime
  
open Fsc.Types
open Fsc.Utils

let discovery obj value  =
    match ctx with
     | _ when !- recognition(obj,ctx?value) ->   retract <| Fsc.Facts.recognition(obj,ctx?value)
                                                 tell <| Fsc.Facts.recognition(obj,value) 
     | _ -> tell <| Fsc.Facts.recognition(obj,value) 

let get_detected obj  =
    match ctx with
     | _ when !- detected(obj) ->  true
     | _ -> false

let get_out  syscmd =  try 
                        let c = ctx?out |- (result(syscmd,ctx?out))
                        c
                       with e-> "not found"


let execute syscmd = async {
          let result = match syscmd with
                          | Prefix "discovery" rest -> sprintf "%s"  (command syscmd ["idphoto",get_out "get photo"])
                          | Prefix "broadcast" rest -> sprintf "%s"  (command "telegram broadcast" ["text",sprintf "%s" ((rest).Trim())])
                          | Prefix "help" rest -> get_out "help"
                          | _ ->  sprintf "%s" (command  syscmd  []) 
          return syscmd,result
          }

let send_message idchat  cmd text cap=
          printfn "send_message %i %s %s" idchat cmd text
          match (cmd) with
                   | Prefix "get photo" rest ->  command (sprintf "telegram photo %i" idchat)
                                                    ["idphoto",text;
                                                     "text",cap]
                   | Prefix "get video" rest ->  command (sprintf "telegram video %i" idchat) 
                                                    ["idvideo",text;
                                                     "text",cap]
                   | Prefix "nop" rest -> sprintf "%s" "nop"
                   | _   -> command  (sprintf "telegram text %i" idchat) [ "text", sprintf "%s -> %s" cmd  text]

let sendresponse idchat res =
 match ctx with
      | _ when !- (request(idchat,ctx?usercmd),usrcmd(ctx?usercmd,ctx?syscmd),result(ctx?syscmd,res)) -> do 
                                             printfn "request(%i,%s)" idchat ctx?syscmd
                                             let result = send_message idchat ctx?syscmd res (caption (get_out "discovery"))
                                             retract<|Fsc.Facts.request(idchat,ctx?usercmd)     
      | _ ->  printf ""

[<CoDa.ContextInit>]
let initFacts () =
 tell <| Fsc.Facts.request(0,"nop")
 tell <| Fsc.Facts.recognition("exit",0.0)

 tell <| Fsc.Facts.confidence("obstacle",0.0,50.0)
 tell <| Fsc.Facts.confidence("person",0.9,1.0)
 tell <| Fsc.Facts.confidence("exit",1.0,1.0)
 tell <| Fsc.Facts.confidence("never",0.0,0.0)

 tell <| Fsc.Facts.cmddesc("get photo","snapshot a photo with camera")
 tell <| Fsc.Facts.cmddesc("get video","shoot a movie with camera")
 tell <| Fsc.Facts.cmddesc("get distance","get the distance of the obstacle")
 tell <| Fsc.Facts.cmddesc("left","rover turn left")
 tell <| Fsc.Facts.cmddesc("right","rover turn right")
 tell <| Fsc.Facts.cmddesc("forward","rover move forward")
 tell <| Fsc.Facts.cmddesc("backward","rover move backward")
 tell <| Fsc.Facts.cmddesc("stop","stop the rover")
 tell <| Fsc.Facts.cmddesc("led on 0", "turn on led number 0")
 tell <| Fsc.Facts.cmddesc("led off 0", "turn off led number 0")
 tell <| Fsc.Facts.cmddesc("help", "command help")

 tell <| Fsc.Facts.usrcmd("photo","get photo")
 tell <| Fsc.Facts.usrcmd("video","get video")  
 tell <| Fsc.Facts.usrcmd("distance","get distance")
 tell <| Fsc.Facts.usrcmd("left","go left")
 tell <| Fsc.Facts.usrcmd("right","go right")
 tell <| Fsc.Facts.usrcmd("lon","led on 0")
 tell <| Fsc.Facts.usrcmd("loff","led off 0")
 tell <| Fsc.Facts.usrcmd("forward","go forward")
 tell <| Fsc.Facts.usrcmd("backward","go backward")
 tell <| Fsc.Facts.usrcmd("stop","stop")
 tell <| Fsc.Facts.usrcmd("discovery","discovery")
 tell <| Fsc.Facts.usrcmd("help","help")

 tell <| Fsc.Facts.action("never","true","discovery")
 tell <| Fsc.Facts.action("never","false","get distance")
 tell <| Fsc.Facts.action("never","false","get channels")
 tell <| Fsc.Facts.action("never","true","get photo")
 tell <| Fsc.Facts.action("indoor","true","led on 0")
 tell <| Fsc.Facts.action("person","false","led off 0")
 tell <| Fsc.Facts.action("obstacle","true","stop")
 tell <| Fsc.Facts.action("never","false","led off 0")


 let mutable help="?"
 for _ in !-- usrcmddesc(ctx?usrcmd,ctx?desc) do
       help <- sprintf "%s\n\t*%s*\t%s" help ctx?usrcmd ctx?desc
 
 tell <| Fsc.Facts.result("help",help)
 for _ in !-- action(ctx?obj,ctx?status,ctx?syscmd) do  discovery ctx?obj  (float(num.MinValue))  

[<CoDa.Context("fsc-ctx")>]
[<CoDa.EntryPoint>]
let main () =
 initFacts ()
 let mutable listresult=[||]
 let mutable array_cmd =  [|"broadcast start"|]


 while (not (get_detected "exit")) do

                                
     for _ in !-- next(ctx?syscmd) do array_cmd <-  array_cmd |> Array.append [|ctx?syscmd|] 

     listresult <- Async.Parallel [for syscmd in  Array.distinct array_cmd  -> execute syscmd] |> Async.RunSynchronously
 
     for r in listresult do
         match r with
          |syscmd,res -> for _ in !-- result(syscmd,ctx?out) do retract <| Fsc.Facts.result(syscmd, ctx?out)   
                         tell <| Fsc.Facts.result(syscmd,res)

     discovery "obstacle" (try 
                            float(get_out "get distance")
                           with e-> 0.0) 
 
     
     let infoimage = get_out "discovery" |> imagerecognition
     for tag in infoimage.tags do 
         discovery tag.name tag.confidence 

     for _ in !-- recognition(ctx?obj,ctx?value) do
      printfn "recognition:%s,\t%f,\t%b" ctx?obj ctx?value (get_detected ctx?obj)
     
 
     for idchat in (get_out "get channels" |> get_list) do
      for _ in !-- response(idchat,ctx?res) do 
          sendresponse idchat ctx?res

      for _ in !-- request(idchat,ctx?usercmd) do
       printfn "not response request:%i,\t%s" idchat ctx?usercmd

      let mutable msg=get_message idchat               
      while (msg.Length >0) do  
       match ctx with
           | _ when !- usrcmd(msg.[0],ctx?syscmd) ->    retract <| Fsc.Facts.request(idchat,msg.[0])
                                                        tell <| Fsc.Facts.request(idchat,msg.[0])
           | _ -> tell <| Fsc.Facts.request(idchat,"help")
       msg<- get_message idchat  
     array_cmd <-[||]
     
do if (conf.debug) then debug()
    else run()