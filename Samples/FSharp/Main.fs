module Main


open Microsoft.Research.Joins
type SC = Join.Scalable
type LB = Join.LockBased
type Flavor = LB

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
   member def.nsync<'a>() = Synchronous.CreateChannel<'a>(def)   
  
type stack<'a> = ('a -> unit) * (unit -> 'a option)

// basic stack, concurrency limited by locking to access 'stack'
let Stack() : stack<'a> = 
  let def = Join.Create<Flavor>()
  let pop, push, stack = def.sync<'a option>(), def.nsync<'a>(), def.async<'a list>()
  def.When(push).And(stack).
    Do(fun x xs -> send stack (x::xs))
  def.When(pop).And(stack).
    Do(function x::xs -> send stack xs; Some(x)
              | []    -> send stack []; None )
  send stack []
  send push, (fun () -> signal pop)

//// testing 
//let push,pop = Stack() 
//send push 1
//send push 2
//printfn "result is %A" (signal pop)

// concurrent loose stack (no timestamps so far)

let depleted = ref 0  
let stolen   = ref 0
 
let Stacks<'a> n : stack<'a>[] =  // allocates n proxies to a concurrent stack
    let def  = Array.init n (fun i -> Join.Create<Flavor>()) 
    let get  = Array.init n (fun i -> def.[i].sync<'a option>())
    let rec steal i j = 
      if i = j then incr depleted; None
      else match signal get.[j] with None   -> steal i ((j+1) % n)  // tail-recursive
                                   | Some x -> incr stolen; Some x 
    Array.mapi 
      ( fun i (d:Join) -> 
          let push  = d.nsync<'a>()
          let pop   = d.sync<'a option>()
          let local = d.async<'a list>()
          def.[i].When(push).And(local).
            Do(fun x xs -> send local (x::xs)) // always available
          def.[i].When(get.[i]).And(local).
            Do(function x::xs -> send local xs; Some x
                      | []    -> send local []; None)
          def.[i].When(pop).And(local).
            Do(function x::xs -> send local xs; Some x             // simple case
                      | []    -> send local []; steal i ((i+1)%n)) // try to steal x from any sibling; menawhile others can still push
          send local []
          (send push, fun () -> signal pop) )
      def

// testing
//let f, g = Stacks<int> 2
//f 0 10
//f 1 20
//f 1 30 
//printfn "got %d" (g 0).Value
//printfn "got %d" (g 0).Value
//()

// 8 threads, 8 local queues, 10 initial local push, then 100,000 random pop/push

let fork (f: int -> unit) x = 
  let t = new System.Threading.Thread(fun () -> f x) 
  t.Start()
  t
let wait (t: System.Threading.Thread) () = t.Join() 

// let toss n i = (n,i).GetHashCode() % 2 = 0 
let toss n i = ((n * 13 + i * 17) / 512) % 2 = 0


let workload caption length stacks = 
  let worker ((push,pop):stack<int>) n =
    //printfn "starting thread #%d" n 
    for i = 0 to length - 1 do 
      if i < 4 || toss n i then push i else ignore(pop()) 
    //printfn "complete thread #%d" n 

  let timer = new System.Diagnostics.Stopwatch()
  timer.Start()
  stacks |>
  Array.mapi (fun i stack -> fork (worker stack) i) |>
  Array.iter (fun (t: System.Threading.Thread) -> t.Join()) 
  timer.Stop()
  printfn "%s %12d" caption timer.ElapsedMilliseconds

do 
  for breadth = 1 to 16 do
    let size = 10000000 
    let length = size / breadth
    printfn "processing %d calls on %d threads (runtime in mS)" size breadth
    workload "single stack    " length (let s = Stack() in Array.create breadth s)
    workload "disjoint stacks " length (Array.init breadth (fun i -> Stack()))
    depleted :=0; stolen :=0; 
    workload "concurrent stack" length (Stacks breadth)
    printfn "(depleted %d, stolen %d)" !depleted !stolen 
  ()

 
 (*
 // other examples in F# 

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



let b =  new Buffer<int>()
let spawn  =  let def = Join.Create()
              let sp =  def.async<unit->unit>()
              def.When(sp).Do(fun f -> f())
              fun f  -> sp.Invoke(f) 



do spawn(fun() -> for i in 1 .. 10 do b.Put(i))

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
  
*)