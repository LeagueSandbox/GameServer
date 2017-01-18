CC := gcc
CFLAGS := -I.
DEPS := callbacks.h enet.h list.h protocol.h time.h types.h unix.h utility.h win32.h
OBJ := callbacks.o host.o list.o packet.o peer.o protocol.o unix.o win32.o

NAME := enet
UNAME := $(shell uname)
ifeq ($(UNAME),Darwin)
	DYLIB := lib$(NAME).dylib
else
	DYLIB := lib$(NAME).so
endif

all: $(DYLIB)

%.o: %.c $(DEPS)
	$(CC) -c -o $@ $< $(CFLAGS)

$(DYLIB): $(OBJ)
ifeq ($(UNAME),Darwin)
	$(CC) -dynamiclib $^ -o $@
else
	$(CC) -shared -Wl,-soname,$@ -o $@ $^
endif
