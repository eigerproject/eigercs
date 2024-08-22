<div align="center">
    <img src="artwork/logo-gray.png" width="400px" />
    <h1>Eiger Programming Language</h1>
    <p>
      Eiger <i>(name from <a href="https://en.wikipedia.org/wiki/Eiger" target="_blank">Mt. Eiger, Switzerland</a>)</i> is a simple interpreted dynamic-typed programming language.
    </p>
    <a href="https://eigerproject.github.io" target="_blank">Website</a>
    |
    <a href="https://eigerproject.github.io/docs" target="_blank">Documentation</a>
    <br><br>
    <img src="https://img.shields.io/github/license/eigerproject/eigercs?label=license">
    <img src="https://img.shields.io/github/repo-size/eigerproject/eigerlang?label=Code%20Size">
    <img src="https://img.shields.io/github/contributors/eigerproject/eigercs?label=contributors">
    <img src="https://github.com/eigerproject/eigerlang/actions/workflows/test.yml/badge.svg">
    <img src="https://img.shields.io/github/stars/eigerproject/eigerlang">
  </div>
  <h3>Current Features</h3>
  <ul>
      <li>Variables</li>
      <li>Control Flow</li>
      <li>Loops</li>
      <li>Functions</li>
      <li>Object Oriented Programming <i>(WIP)</i></li>
  </ul>
  <h2 align="center">Installation</h2>
  <i><p align="center">
    There are no stable releases of the language yet, but you can still build from the source
  </p></i>
  <h2 align="center">Building and Running</h2>
  <ul>
      <li>Clone this repository</li>
      <li>Open the solution `.sln` file using Visual Studio</li>
      <li>Build and run</li>
  </ul>
  <h2 align="center">Simple Example</h2>
  
  ```
  class Person
      let private name
      let private surname

      ~ constructor
      func new(name, surname)
          this.name = name
          this.surname = surname
      end
  
      func Introduction()
          emitln("I am " + this.name + " " + this.surname + "!")
      end
  
      func Greet(other)
          emitln(this.name + " greeted " + other.name + "!")
      end
  end
  
  let px = Person("Name1", "Surname1")
  let py = Person("Name2", "Surname2")
  
  px.Introduction()
  py.Introduction()
  px.Greet(py)
  
  Person("a","b").Introduction()
  ```
  
  This is a very simple example showing Eiger's OOP capabilities
  
