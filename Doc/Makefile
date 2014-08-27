PDFLATEX = "C:\Program Files (x86)\MiKTeX 2.9\miktex\bin\pdflatex.exe"
BIBTEX = "C:\Program Files (x86)\MiKTeX 2.9\miktex\bin\bibtex.exe"
tutorial.pdf: tutorial.tex 
    $(PDFLATEX) Tutorial.tex
	$(BIBTEX)  Tutorial
	$(PDFLATEX) Tutorial.tex
	$(PDFLATEX) Tutorial.tex
    copy tutorial.pdf ..\Documentation\Tutorial.pdf

clean: 
	del /Q /A-R *.aux *.pdf *.log *.bbl *.blg
        
rebuild: clean tutorial.pdf


