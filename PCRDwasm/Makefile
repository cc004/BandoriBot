%.o: %.c
	gcc -c -O2 $^ -o $@
%.o: %.cpp
	g++ -c -O2 $^ -o $@
pcrdwasm.dll: pcrd.o main.o wasm-rt.o wasm-rt-impl.o
	g++ $^ --shared -o $@

all: pcrdwasm.dll