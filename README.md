<div>
    <img src="artwork/eiger-green-512.png" width="400px" />
    <h1>Eiger Programming Language</h1>
    <p>
      Eiger <i>(name from <a href="https://en.wikipedia.org/wiki/Eiger" target="_blank">Mt. Eiger, Switzerland</a>)</i> is a multi-paradigm programming language with a dynamic type system.
    </p>
    <a href="https://eigerproject.github.io" target="_blank">Website</a>
    |
    <a href="https://eigerproject.github.io/docs" target="_blank">Documentation</a>
    <br><br>
  </div>

> [!NOTE]
> This is the older, C# implementation of the language, please refer to [the EigerC repository](https://github.com/eigerproject/eigerc.git)

  <h2>Building and Running</h2>
  <h3>If using Visual Studio</h3>
  <ul>
      <li>Clone this repository</li>
      <li>Open the solution file using Visual Studio</li>
      <li>Build and run</li>
      <li>To run unit tests, use the Test Explorer</li>
  </ul>
  <h3>Using the .NET CLI</h3>
  <ul>
      <li>Clone this repository</li>
      <li>Run <code>dotnet run</code> in <code>/eiger</code></li>
      <li>To run unit tests, run <code>dotnet test</code> in <code>/eiger.Tests</code></li>
  </ul>
  <h2>Simple Example/Syntax Showcase</h2>
  
```
  class Person {
	let name
	let surname

    func new(name, surname) {
    	this.name = name
    	this.surname = surname
    }

    func Introduction() {
    	emitln("I am " + this.name + " " + surname + "!")
    }

    func Greet(other) {
    	emitln(name + " greeted " + other.name + "!")
    }

}

let px = Person("Name1", "Surname1")
let py = Person("Name2", "Surname2")

px.Introduction()
py.Introduction()

px.Greet(py)
```
