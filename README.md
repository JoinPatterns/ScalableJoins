#A Scalable Joins Library

Scalable Joins is a CLR library for declarative, scalable parallel synchronization.

##Requirements

.NET 4.5
Visual Studio 2013 (optional)
pdflatex (optional)

##Background

With multiple cores, even mainstream programmers need to write parallel code.  Those parallel threads need to communicate and synchronize.

Writing correct synchronization code is hard; achieving performance that scales with the number of cores is a black art.

Scalable Joins is a library that extends C# and other Common Language Runtime languages with declarative, high-level synchronization constructs.

The model is based on the message passing join calculus:
* Objects can declare channels on which to receive messages and requests;
* Methods are declared to react when sets of channels are filled.

Threads coordinate by sending messages. The ideas are simple, yet powerful.

The library is a new, lock-free  re-implementation of the lock-based Joins concurrency library, designed to scale on parallel hardware. As before, programmers write simple, high-level code but now they also get the parallel scalability of complex, low-level code previously crafted by experts.
The library provides both lock-based and lock-free implementations of join patterns. Its modular design supports experimentation with other implementation strategies.  

##Example

Consider E. Dijkstra's classic Dining Philosophers problem. Declarative join patterns, provided by our Joins library, make it almost trivial to solve - all we have to do is state the desired constraints:

```csharp
var table = Join.Create(2 * n); 
 
// channel arrays for requests and resources
Synchronous.Channel[] hungry; table.Initialize(out hungry, n);
Asynchronous.Channel[] forks; table.Initialize(out forks, n);
for (int i = 0; i < n; i++) { 
 var leftFork = forks[i]; var rightFork = forks[(i + 1) % n];
 // a join pattern
 table.When(hungry[i]).And(leftFork).And(rightFork).Do(() => {
    // eat ... 
    leftFork(); rightFork(); // replace forks 
    // think ...
 });
}
 
// set the table 
foreach (var fork in forks) fork();  
 
// spawn the philosophers
for (int i = 0; i < n; i++) { var _i = i; 
  var phil = new Thread(() => {   
    while (true) hungry[_i](); // request to eat
  });
  phil.Start();
}
```

This code represents the resources of the problem (the forks) as asynchronous channels. A resource is available if, and only if, there is a message on its channel. When hungry, philosopher `i` makes a synchronous request on his dedicated channel, `hungry[i]`.
A request on `hungry[i]` will wait until or unless the philosopher's left and right forks are available. Once done eating, a philosopher releases his forks by invoking their channels, replenishing the resources he just consumed.

##Building

With VS2013 simply open the solution file Joins.sln. 

This contains projects for the library as well as larger demos, smaller samples and tutorial style documentation.

##Links

Papers describing the implementation(s) and performance evaluation are available here:


Aaron Turon and Claudio Russo.  *Scalable Join Patterns*. *Proceedings of the 2011 ACM International Conference on
  Object Oriented Programming Systems Languages and Applications*, OOPSLA '11,
  pages 575-594, New York, NY, USA, 2011. ACM.
  [pdf | (http://www.research.microsoft.com/~crusso/papers/scalablejoins.pdf) ]  

Claudio Russo.
  *The Joins Concurrency Library*.
  *Ninth International Symposium on
  Practical Aspects of Declarative Languages (PADL 2007)*, volume 4354 of *
  Lecture Notes In Computer Science (LNCS)*, pages 260-274. Springer-Verlag,
  January 2007.
  (c) Springer-Verlag,
  [pdf | (http://research.microsoft.com/~crusso/papers/padl07.pdf")]| 


More generally, see Wikipedia's [join pattern page](https://en.wikipedia.org/wiki/Join-pattern).


##Credits

The primary authors are Claudio Russo, Aaron Turon and Matthew Parkinson. Thanks also to Nick Benton, Cedric Fournet, Dan Alistarh and Lucas Bordeaux.
