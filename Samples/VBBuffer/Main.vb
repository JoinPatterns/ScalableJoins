
'------------------------------------------------------------------------------
' <copyright file="Main.vb" company="Microsoft">
'     Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
' <disclaimer>
'     THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
'     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
'     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
'     PURPOSE.
' </disclaimer>
'------------------------------------------------------------------------------
Module MainModule
  Sub Main()
    BufferClient.Test()
    OnePlaceBufferClient.Test()
    System.Console.WriteLine("Hit return to exit")
    System.Console.ReadLine()
  End Sub
End Module

Module BufferClient
  Dim Buffer As Buffer(Of String) = New Buffer(Of String)()

  Sub Producer()
    Console.WriteLine("Producer started")
    For i As Integer = 0 To 20
      Buffer.Put(i.ToString())
      Console.WriteLine("Produced!{0}", i)
      Thread.Sleep(10)
    Next
  End Sub

  Sub Consumer()
    Console.WriteLine("Consumer started")
    For i As Integer = 0 To 20
      Dim s As String = Buffer.Take()
      Console.WriteLine("Consumed!{0}", i)
      Thread.Sleep(8)
    Next
    Console.WriteLine("Done consuming.")
  End Sub

  Sub Test()
    Dim ProducerThread As Thread = New Thread(New ThreadStart(AddressOf Producer))
    Dim ConsumerThread As Thread = New Thread(New ThreadStart(AddressOf Consumer))
    System.Console.WriteLine("Testing Buffer.")
    ProducerThread.Start()
    ConsumerThread.Start()
    ConsumerThread.Join()
  End Sub

End Module

Module OnePlaceBufferClient
  Dim Buffer As OnePlaceBuffer(Of String) = New OnePlaceBuffer(Of String)()

  Sub Producer()
    Console.WriteLine("Producer started")
    For i As Integer = 0 To 20
      Buffer.Put(i.ToString())
      Console.WriteLine("Produced!{0}", i)
      Thread.Sleep(10)
    Next
  End Sub

  Sub Consumer()
    Console.WriteLine("Consumer started")
    For i As Integer = 0 To 20
      Dim s As String = Buffer.Take()
      Console.WriteLine("Consumed!{0}", i)
      Thread.Sleep(8)
    Next
    Console.WriteLine("Done consuming.")
  End Sub

  Sub Test()
    Dim ProducerThread As Thread = New Thread(New ThreadStart(AddressOf Producer))
    Dim ConsumerThread As Thread = New Thread(New ThreadStart(AddressOf Consumer))
    System.Console.WriteLine("Testing OnePlaceBuffer.")
    ProducerThread.Start()
    ConsumerThread.Start()
    ConsumerThread.Join()  
  End Sub

End Module

