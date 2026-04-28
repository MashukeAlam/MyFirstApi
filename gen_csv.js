const fs = require('fs');
const path = require('path');

const OUTPUT_FILE = path.resolve(process.cwd(), 'items.csv'); // write to current working dir
const TOTAL_ROWS = 10000;

const ITEMS = ['Rice', 'Sugar', 'Flour', 'Wheat', 'Salt', 'Oil'];

const randomFrom = (arr) => arr[Math.floor(Math.random() * arr.length)];

const generateRow = (index) => {
  const name = randomFrom(ITEMS);
  const code = `ITM${String(index).padStart(5, '0')}`;
  return `${name},${code},KG,raw,${Math.random() > 0.5 ? 5 : 0},0`;
};

(async () => {
  console.log('Writing to:', OUTPUT_FILE);

  const stream = fs.createWriteStream(OUTPUT_FILE);

  stream.write('Name,Code,Unit,ItemType,VatRate,SdRate\n');

  for (let i = 1; i <= TOTAL_ROWS; i++) {
    if (!stream.write(generateRow(i) + '\n')) {
      await new Promise(resolve => stream.once('drain', resolve));
    }
  }

  stream.end();

  stream.on('finish', () => {
    console.log('✅ CSV generated successfully');
  });

  stream.on('error', (err) => {
    console.error('❌ Error writing file:', err);
  });
})();