using System;
using System.Collections.Generic;

namespace tetris
{
    public class BlockQueue
    {
        // available blocks to pick from
        private readonly Block[] blocks = new Block[]
        {
            new IBlock(),
            new JBlock(),
            new LBlock(),
            new OBlock(),
            new SBlock(),
            new TBlock(),
            new ZBlock()
        };

        private readonly Random random = new Random(Guid.NewGuid().GetHashCode());  // more randomness for first block in games started at the same time (two player mode)

        // preview of the next coming blocks
        public List<Block> NextBlocks { get; private set; }

        // picks a random block
        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        // initialize by generating next 3 blocks
        public BlockQueue()
        {
            NextBlocks = new List<Block>();

            // generate first 3 blocks to queue
            for (int i = 0; i < 3; i++)
            {
                NextBlocks.Add(RandomBlock());
            }
        }

        // returns the next block, removes it and generates a new one
        public Block GetAndUpdate()
        {
            Block block = NextBlocks[0];
            NextBlocks.RemoveAt(0);
            NextBlocks.Add(RandomBlock());

            // pick a new block different from previous
            while(NextBlocks[1].Id == NextBlocks[2].Id)
            {
                NextBlocks.RemoveAt(2);
                NextBlocks.Add(RandomBlock());
            }
            while(NextBlocks[1].Id == NextBlocks[2].Id);

            return block;
        }
    }
}
