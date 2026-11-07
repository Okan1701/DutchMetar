import { HeadingPipe } from './heading-pipe';

describe('HeadingPipe', () => {
    const pipe = new HeadingPipe();

    it('pads 0 to three digits', () => {
        expect(pipe.transform(0)).toBe('000');
    });

    it('pads 7 to three digits', () => {
        expect(pipe.transform(7)).toBe('007');
    });

    it('pads 93 to three digits', () => {
        expect(pipe.transform(93)).toBe('093');
    });

    it('preserves 100', () => {
        expect(pipe.transform(100)).toBe('100');
    });

    it('preserves 359', () => {
        expect(pipe.transform(359)).toBe('359');
    });
});
