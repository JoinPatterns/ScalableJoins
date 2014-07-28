module Main


open Microsoft.Research.Joins

// some tentative helper functions to avoid F#'s  c.Invoke() syntax for channel sends
// perhaps we should just stick to c.Invoke()
let inline signal (c:^c)  = (^c : (member Invoke : unit -> 'b) c)
let inline send (c:^c) a  = (^c : (member Invoke : 'a -> 'b) (c,a))
let inline (<<) (c:^c) a  = (^c : (member Invoke : 'a -> 'b) (c,a))
let inline (<!) (c:^c) ()  = (^c : (member Invoke : unit -> 'b) c)
let test (c:Asynchronous.Channel) = c <! ()

type Join with 
   member def.async() = Asynchronous.CreateChannel(def)
   member def.sync() = Synchronous.CreateChannel(def)    
   member def.async<'a>() = Asynchronous.CreateChannel<'a>(def)
   member def.sync<'a,'r>() = Synchronous<'r>.CreateChannel<'a>(def)
   member def.sync<'a>() = Synchronous<'a>.CreateChannel(def)   
  
let buffer<'a>() =
    let def = Join.Create(2)
    let put = Asynchronous.CreateChannel<'a>(def)
    let get = Synchronous<'a>.CreateChannel(def)
    def.When(get).And(put).Do(fun a -> a)
    (put,get)

let buf<'a>() =
    let def = Join.Create()
    let put = def.async<'a>()
    let get = def.sync<'a>()
    def.When(get).And(put).Do(fun a -> a)
    (put,get)

type Buffer<'a>() =
    let def = Join.Create()
    let put = def.async<'a>()
    let get = def.sync<'a>()
    do def.When(get).And(put).Do(fun a -> a)
    member this.Put(a) = send put a
    member this.Get() = signal get

let spawn f = let def = Join.Create()
              let spawn =  def.async<unit->unit>()
              def.When(spawn).Do(fun f -> f())
              spawn.Invoke(f) 

let b =  new Buffer<int>()

for i in 1 .. 10 do spawn(fun() -> b.Put(i))

for i in 1..10 do printf "\n%i" (b.Get())

do System.Console.ReadKey()


let phils n =
  let def = Join.Create(2*n)
  let fs = [|for i in 1..n -> def.async() |]
  let ps = [|for i in 1..n -> def.sync() |]
  for i in 0..n-1 do
    let left = fs.[i]
    let right = fs.[(i+1) % n]
    def.When(ps.[i]).And(left).And(right).Do(fun () ->
       signal left
       signal right)
  for f in fs do signal f
  ps

let lock() =
    let def = Join.Create(2)
    let acquire,release = def.sync(),def.async()
    def.When(acquire).And(release).Do(fun () -> ())
    signal release
    acquire,release

let semaphore n =
    let def = Join.Create(2)
    let acquire,release = def.sync(),def.async()
    def.When(acquire).And(release).Do(fun () -> ())
    for i in 1 .. n do signal release
    acquire,release

let rendezvous() =
    let def = Join.Create(2)
    let left,right = def.sync(),def.sync()
    def.When(left).And(right).Do(fun () -> ())
    left,right


let exchange<'a,'b>() =
    let def = Join.Create(2)
    let left,right = def.sync<'a,'a*'b>(),def.sync<'b,'a*'b>()
    let d = def.When(left).And(right).Do(fun a b -> (a,b))
    (fun a -> snd (send left a)),
     (fun b -> fst (send right b))


let Barrier(n) =
    let def = Join.Create(n)
    let chans = [|for i in 1..n -> def.sync() |]
    let (Some pat) = 
        chans |> Array.fold(fun p c  -> match p with None -> Some (def.When(c)) | Some p -> Some(p.And(c))) None 
    pat.Do(fun()-> ())
    chans

let Barrier'(n) =
    let def = Join.Create(n)
    let chans = [|for i in 1..n -> def.sync() |]
    def.When(chans).Do(fun () -> ())
    chans

(* TBC - see Scalable Joins paper
let TreeBarrier'(n) =
    let def = Join.Create(n)
    let chans = [|for i in 1..n -> def.sync() |]
    def.When(chans).Do(fun () -> ())
    chans
 *)

let ps = phils 100
let ds = Barrier(ps.Length)
let life i = fun () -> for j in 1..10 do 
                         signal (ps.[i]); 
                         printfn "%i,%i" i j 
                       signal (ds.[i])
                       printfn "%i:Done" i
ps |> Array.iteri (fun i p -> spawn (life i))

do System.Console.ReadKey()
 

let fork (p:Synchronous.Channel) = spawn (fun () -> signal p)

let phils5() =
  
  let def = Join.Create()
  
  let [| f1;f2;f3;f4;f5 |] = [|for i in 1..5 -> def.async() |]
  let [| p1;p2;p3;p4;p5 |] = [|for i in 1..5 -> def.sync() |]
  
  def.When(p1).And(f1).And(f2).Do(fun () -> signal f1; signal f2; signal p1)
  def.When(p2).And(f2).And(f3).Do(fun () -> signal f2; signal f3; signal p2)
  def.When(p3).And(f3).And(f4).Do(fun () -> signal f4; signal f5; signal p3)
  def.When(p4).And(f4).And(f5).Do(fun () -> signal f4; signal f5; signal p4)
  def.When(p5).And(f5).And(f1).Do(fun () -> signal f5; signal f1; signal p4)

  signal f1;signal f2; signal f3; signal f4;signal f5
  fork p1; fork p2; fork p3; fork p4
  
